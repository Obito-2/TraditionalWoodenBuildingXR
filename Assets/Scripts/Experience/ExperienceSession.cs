/// <summary>
/// 当前体验会话：最近一次加载的 Addressables 键、目录项（用于场景名与 LLM 文案）。
/// 不挂物体，跨场景由 Main DontDestroyOnLoad 与静态状态配合使用。
/// </summary>
public static class ExperienceSession
{
    public static string LastAddressableKey { get; private set; } = "DouGong";

    public static ExperienceModelEntry ActiveEntry { get; private set; }

    public static void BeginExperience(ExperienceModelEntry entry, string addressableKeyUsed)
    {
        ActiveEntry = entry;
        if (!string.IsNullOrEmpty(addressableKeyUsed))
            LastAddressableKey = addressableKeyUsed;
        else if (entry != null && !string.IsNullOrEmpty(entry.addressableKey))
            LastAddressableKey = entry.addressableKey;
    }

    public static string HubSceneName =>
        ActiveEntry != null && !string.IsNullOrEmpty(ActiveEntry.hubSceneName)
            ? ActiveEntry.hubSceneName
            : "DouGong";

    public static string JigsawSceneName =>
        ActiveEntry != null && !string.IsNullOrEmpty(ActiveEntry.jigsawSceneName)
            ? ActiveEntry.jigsawSceneName
            : "DouGongJigsaw";

    public static string GetLlmUserQuestionPrefix()
    {
        if (ActiveEntry != null && !string.IsNullOrEmpty(ActiveEntry.llmUserQuestionPrefix))
            return ActiveEntry.llmUserQuestionPrefix;
        return "请介绍下八铺作补间铺作构件中的：";
    }

    public static bool TryGetLlmSystemPromptOverride(out string prompt)
    {
        prompt = null;
        if (ActiveEntry == null || string.IsNullOrEmpty(ActiveEntry.llmSystemPromptOverride))
            return false;
        prompt = ActiveEntry.llmSystemPromptOverride;
        return true;
    }
}
