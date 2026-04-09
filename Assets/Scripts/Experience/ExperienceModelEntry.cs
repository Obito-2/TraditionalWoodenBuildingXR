using System;
using UnityEngine;

/// <summary>
/// 单个可体验模型在目录中的配置（Addressables 键、场景名、LLM 前缀等）。
/// </summary>
[Serializable]
public class ExperienceModelEntry
{
    [Tooltip("Addressables 地址/标签对应的资源键")]
    public string addressableKey = "DouGong";

    [Tooltip("主菜单按钮等 UI 用显示名；可空则用 addressableKey")]
    public string displayName = "";

    public Vector3 spawnPosition = new Vector3(0f, 0f, 1f);

    [Tooltip("从拼图返回时加载的 Hub 场景名")]
    public string hubSceneName = "DouGong";

    [Tooltip("进入拼图模式时加载的场景名")]
    public string jigsawSceneName = "DouGongJigsaw";

    [TextArea(1, 3)]
    [Tooltip("拼到用户问题前的固定前缀，例如「请介绍下…构件中的：」")]
    public string llmUserQuestionPrefix = "请介绍下八铺作补间铺作构件中的：";

    [TextArea(2, 12)]
    [Tooltip("非空时覆盖 LLMChat 上 Inspector 中的系统提示词")]
    public string llmSystemPromptOverride = "";

    public static ExperienceModelEntry CreateFallback(string addressableKey)
    {
        return new ExperienceModelEntry
        {
            addressableKey = addressableKey,
            displayName = addressableKey,
            spawnPosition = new Vector3(0f, 0f, 1f),
            hubSceneName = "DouGong",
            jigsawSceneName = "DouGongJigsaw",
            llmUserQuestionPrefix = "请介绍下八铺作补间铺作构件中的：",
            llmSystemPromptOverride = ""
        };
    }
}
