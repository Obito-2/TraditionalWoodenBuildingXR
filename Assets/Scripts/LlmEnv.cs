using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 从 .env 读取 LLM_API_URL、LLM_API_KEY、LLM_MODEL（仅文件，不回退进程环境变量）。
/// 查找顺序：项目根目录 .env（与 Assets 同级）→ StreamingAssets/.env（便于随包携带）。
/// </summary>
public static class LlmEnv
{
    const string ApiUrlName = "LLM_API_URL";
    const string ApiKeyName = "LLM_API_KEY";
    const string ModelName = "LLM_MODEL";

    static Dictionary<string, string> _fileVars;
    static bool _loaded;

    static void EnsureLoaded()
    {
        if (_loaded)
            return;
        _loaded = true;
        _fileVars = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

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
                foreach (string line in File.ReadAllLines(path))
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
            catch (Exception e)
            {
            }

            break;
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
        return u;
    }

    public static string ApiKey => Get(ApiKeyName).Trim();

    public static string Model => Get(ModelName).Trim();

    /// <summary>RAG 智能体接口 URL，形如 http://192.168.x.x:8000/ar/chat</summary>
    const string RagApiUrlName = "RAG_API_URL";
    public static string RagApiUrl => Get(RagApiUrlName).Trim();
}
