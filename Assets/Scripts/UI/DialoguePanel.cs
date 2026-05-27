using System;
using System.Collections.Generic;
using CodeArchitect.Manager.Event;
using CodeArchitect.Manager.Image;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class DialoguePanel : BaseFadePanel
{
    private TextMeshProUGUI queryContent;
    private ScrollRect responseScroll;
    private Transform contentContainer;  // ResponseScroll/Viewport/Content

    // 运行时动态创建的内容块（按序排列：文本块 + 图片块）
    private readonly List<GameObject> contentBlocks = new List<GameObject>();
    private Button closeButton;

    // 从预制体现有 TMP 继承的中文字体资产
    private TMPro.TMP_FontAsset cachedFontAsset;

    // TMP 默认字体大小
    private const float DefaultFontSize = 5f;

    protected override void Awake()
    {
        base.Awake();
        queryContent = transform.Find("queryContent").GetComponent<TextMeshProUGUI>();
        cachedFontAsset = queryContent.font;
        contentContainer = transform.Find("ResponseScroll/Viewport/Content");
        responseScroll = transform.Find("ResponseScroll").GetComponent<ScrollRect>();
        closeButton = transform.GetComponentInChildren<Button>();
        closeButton.onClick.AddListener(() => HideSelf());

        // 确保 ImageLoader 被初始化
        EnsureImageLoaderExists();

        // 监听事件
        EventCenter.Instance.AddListener<string>(EventName.AIChat, OnAIChat);
        EventCenter.Instance.AddListener<string>(EventName.LLMResponse, OnLLMResponse);
    }

    private void EnsureImageLoaderExists()
    {
        if (ImageLoader.Instance == null)
        {
            GameObject loaderGo = new GameObject("ImageLoader");
            loaderGo.AddComponent<ImageLoader>();
        }
    }

    private void OnAIChat(string modelInfo)
    {
        // 面板已关闭或正在淡出 → 重新显示
        if (!isShow || !gameObject.activeInHierarchy)
        {
            Show();
        }

        queryContent.text = ExperienceSession.GetLlmUserQuestionPrefix() + modelInfo;

        // 清除旧内容块，显示加载提示
        ClearContentBlocks();
        CreateTextBlock("AI 搜索中，请耐心等待…");
    }

    private void OnLLMResponse(string message)
    {
        // 清除旧内容块（包括 "搜索中" 提示）
        ClearContentBlocks();

        // 解析 Markdown 为内容块列表
        List<MarkdownBlock> blocks = MarkdownConverter.ParseBlocks(message);

        if (blocks.Count == 0)
        {
            // 无图片语法时，整个消息作为纯文本块
            CreateTextBlock(message);
        }
        else
        {
            foreach (var block in blocks)
            {
                switch (block.Type)
                {
                    case MarkdownBlock.BlockType.Text:
                        CreateTextBlock(block.Content);
                        break;

                    case MarkdownBlock.BlockType.Image:
                        CreateImageBlock(block.Content, block.ImageUrl);
                        break;
                }
            }
        }

        // 滚回顶部
        Canvas.ForceUpdateCanvases();
        responseScroll.verticalNormalizedPosition = 1f;
    }

    /// <summary>
    /// 创建文本内容块（TMP + LayoutElement）
    /// </summary>
    private GameObject CreateTextBlock(string markdown)
    {
        string richText = MarkdownConverter.Convert(markdown.Trim());
        richText = SanitizeTextForFont(richText);

        GameObject go = new GameObject("TextBlock");
        go.transform.SetParent(contentContainer, false);

        // 让 VerticalLayoutGroup 能正确控制宽度
        var layout = go.AddComponent<LayoutElement>();
        layout.flexibleWidth = 1;

        var text = go.AddComponent<TextMeshProUGUI>();
        text.font = cachedFontAsset;
        text.text = richText;
        text.fontSize = DefaultFontSize;
        text.raycastTarget = false;

        return go;
    }

    /// <summary>
    /// 过滤掉 TMP 字体资产中不支持的字符，避免 MissingCharacter 警告
    /// </summary>
    private string SanitizeTextForFont(string text)
    {
        if (string.IsNullOrEmpty(text) || cachedFontAsset == null)
            return text;

        // 快速路径：空字符串或无字体时直接返回
        var chars = text.ToCharArray();
        bool hasMissing = false;

        for (int i = 0; i < chars.Length; i++)
        {
            if (!cachedFontAsset.HasCharacter(chars[i], searchFallbacks: true, tryAddCharacter: false))
            {
                hasMissing = true;
                break;
            }
        }

        if (!hasMissing) return text;

        // 存在缺失字符时构建新字符串，缺失字符用 □ 替代
        var sb = new System.Text.StringBuilder(text.Length);
        foreach (char c in text)
        {
            sb.Append(cachedFontAsset.HasCharacter(c, searchFallbacks: true, tryAddCharacter: false)
                ? c
                : '\u25A1');
        }
        return sb.ToString();
    }

    /// <summary>
    /// 创建图片内容块（Image + AspectRatioFitter + 标题 TMP）
    /// 容器内用 VerticalLayoutGroup + ContentSizeFitter 自上而下排列 Title 和 Image
    /// </summary>
    private GameObject CreateImageBlock(string title, string imageUrl)
    {
        // 外层容器——参与父级 VerticalLayoutGroup 布局
        GameObject container = new GameObject("ImageBlock");
        container.transform.SetParent(contentContainer, false);
        contentBlocks.Add(container);

        var containerLayout = container.AddComponent<LayoutElement>();
        containerLayout.flexibleWidth = 1;

        // 容器内部垂直排列 Title + Image
        // childControlHeight = false：让子物体自行控制高度，避免与 AspectRatioFitter 冲突
        var vlg = container.AddComponent<VerticalLayoutGroup>();
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.spacing = 2;

        var fitter = container.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        // --- 标题 ---
        if (!string.IsNullOrEmpty(title))
        {
            GameObject titleGo = new GameObject("Title");
            titleGo.transform.SetParent(container.transform, false);

            // ContentSizeFitter 让标题根据文字内容自动撑高
            var titleFitter = titleGo.AddComponent<ContentSizeFitter>();
            titleFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            titleFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            var titleLayout = titleGo.AddComponent<LayoutElement>();
            titleLayout.flexibleWidth = 1;

            var titleText = titleGo.AddComponent<TextMeshProUGUI>();
            titleText.font = cachedFontAsset;
            titleText.text = title;
            titleText.fontSize = DefaultFontSize;
            titleText.alignment = TextAlignmentOptions.BottomLeft;
            titleText.raycastTarget = false;
        }

        // --- 图片 ---
        GameObject imageGo = new GameObject("Image");
        imageGo.transform.SetParent(container.transform, false);

        var imageFitter = imageGo.AddComponent<AspectRatioFitter>();
        imageFitter.aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;
        // 极小初始宽高比——图片加载前占位高度可忽略，避免撑出巨大空白
        imageFitter.aspectRatio = 0.01f;

        var imageLayout = imageGo.AddComponent<LayoutElement>();
        imageLayout.flexibleWidth = 1;

        var image = imageGo.AddComponent<Image>();
        image.raycastTarget = false;
        image.preserveAspect = true;
        image.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);  // 占位色

        // 异步加载图片
        ImageLoader.Instance.LoadImage(imageUrl, (texture) =>
        {
            if (texture != null && image != null && imageGo != null)
            {
                image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                image.color = Color.white;

                float aspectRatio = (float)texture.width / texture.height;
                imageFitter.aspectRatio = aspectRatio;

                Debug.Log($"[DialoguePanel] 图片已加载: {title} ({imageUrl})");
            }
            else
            {
                Debug.LogWarning($"[DialoguePanel] 图片加载失败或已销毁: {imageUrl}");
            }
        });

        return container;
    }

    private void ClearContentBlocks()
    {
        // 销毁所有动态创建的子物体（跳过非动态创建的如 Scrollbar 等）
        foreach (Transform child in contentContainer)
        {
            // 保留预制体自带的非动态物体（通过名字特征判断）
            Destroy(child.gameObject);
        }
        contentBlocks.Clear();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        EventCenter.Instance.RemoveListener<string>(EventName.AIChat, OnAIChat);
        EventCenter.Instance.RemoveListener<string>(EventName.LLMResponse, OnLLMResponse);
        ClearContentBlocks();
    }
}