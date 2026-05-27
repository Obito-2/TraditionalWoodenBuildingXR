using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 从 Resources/llm_config 或 .env 文件读取 LLM_API_URL、LLM_API_KEY、LLM_MODEL、RAG_API_URL。
/// Resources.Load 是跨平台主路径（Editor + APK 均可用）；文件读取仅 Editor 兜底。
/// </summary>
public static class LlmEnv
{
    const string ApiUrlName = "LLM_API_URL";
    const string ApiKeyName = "LLM_API_KEY";
    const string ModelName = "LLM_MODEL";
    const string RagApiUrlName = "RAG_API_URL";

    static Dictionary<string, string> _fileVars;
    static bool _loaded;

    static void EnsureLoaded()
    {
        if (_loaded)
            return;
        _loaded = true;
        _fileVars = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // 主路径：Resources/llm_config.txt（跨平台，APK 内也可用）
        TextAsset configAsset = Resources.Load<TextAsset>("llm_config");
        if (configAsset != null)
        {
            ParseDotEnvContent(configAsset.text);
            Debug.Log("[LlmEnv] 已从 Resources/llm_config 加载配置");
            return;
        }

        // 兜底：文件系统 .env（仅 Editor / 开发环境可用）
        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string[] candidates =
        {
            Path.Combine(projectRoot, ".env"),
            Path.Combine(Application.streamingAssetsPath, ".env"),
        };

        foreach (string path in candidates)
        {
            if (!File.Exists(path))
                continue;
            try
            {
                ParseDotEnvContent(File.ReadAllText(path));
                Debug.Log($"[LlmEnv] 已从 {path} 加载配置");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[LlmEnv] 读取 {path} 失败: {e.Message}");
            }
            break;
        }
    }

    static void ParseDotEnvContent(string content)
    {
        foreach (string line in content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None))
        {
            string t = line.Trim();
            if (t.Length == 0 || t[0] == '#')
                continue;
            int eq = t.IndexOf('=');
            if (eq <= 0)
                continue;
            string key = t.Substring(0, eq).Trim();
            string val = t.Substring(eq + 1).Trim();
            if (val.Length >= 2 &&
                ((val[0] == '"' && val[^1] == '"') || (val[0] == '\'' && val[^1] == '\'')))
                val = val.Substring(1, val.Length - 2);
            _fileVars[key] = val;
        }
    }

    static string Get(string name)
    {
        EnsureLoaded();
        if (_fileVars != null && _fileVars.TryGetValue(name, out string v))
            return v ?? "";
        return "";
    }

    /// <summary>完整 POST URL（OpenAI 兼容一般为 …/v1/chat/completions）。若以 …/v1 结尾会自动补全路径。</summary>
    public static string ApiUrl => NormalizeChatCompletionsUrl(Get(ApiUrlName).Trim());

    static string NormalizeChatCompletionsUrl(string u)
    {
        if (string.IsNullOrEmpty(u))
            return u;
        if (u.IndexOf("chat/completions", StringComparison.OrdinalIgnoreCase) >= 0)
            return u;
        string t = u.TrimEnd('/');
        if (t.EndsWith("/v1", StringComparison.OrdinalIgnoreCase))
            return t + "/chat/completions";
        // 裸域名/URL 自动补全 /v1/chat/completions
        return t + "/v1/chat/completions";
    }

    public static string ApiKey => Get(ApiKeyName).Trim();

    public static string Model => Get(ModelName).Trim();

    public static string RagApiUrl => Get(RagApiUrlName).Trim();
}