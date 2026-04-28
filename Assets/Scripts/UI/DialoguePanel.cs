using System;
using System.Collections;
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
    private TextMeshProUGUI responseContent;
    private ScrollRect responseScroll;
    private Transform contentContainer;  // ResponseScroll/Viewport/Content
    private Button closeButton;

    // 图片显示相关
    private GameObject imageItemPrefab;
    private List<GameObject> displayedImages = new List<GameObject>();

    protected override void Awake()
    {
        base.Awake();
        queryContent = transform.Find("queryContent").GetComponent<TextMeshProUGUI>();
        contentContainer = transform.Find("ResponseScroll/Viewport/Content");
        responseContent = contentContainer.Find("responseContent").GetComponent<TextMeshProUGUI>();
        responseScroll = transform.Find("ResponseScroll").GetComponent<ScrollRect>();
        closeButton = transform.GetComponentInChildren<Button>();
        closeButton.onClick.AddListener(() => HideSelf());

        // 创建图片项目预制体（动态生成）
        CreateImageItemPrefab();

        // 确保 ImageLoader 被初始化
        EnsureImageLoaderExists();

        //监听aiChat点击事件和模型response事件
        EventCenter.Instance.AddListener<String>(EventName.AIChat, OnAIChat);
        EventCenter.Instance.AddListener<String>(EventName.LLMResponse, OnLLMResponse);
        EventCenter.Instance.AddListener<List<ImageInfo>>(EventName.LLMImages, OnLLMImages);
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
        queryContent.text = ExperienceSession.GetLlmUserQuestionPrefix() + modelInfo;
        responseContent.text = "AI 搜索中，请耐心等待…";
        ClearImages();
    }

    private void OnLLMResponse(string message)
    {
        responseContent.text = MarkdownConverter.Convert(message);
        // 内容更新后滚回顶部
        Canvas.ForceUpdateCanvases();
        responseScroll.verticalNormalizedPosition = 1f;
    }

    private void OnLLMImages(List<ImageInfo> images)
    {
        ClearImages();
        if (images == null || images.Count == 0)
            return;

        foreach (var imageInfo in images)
        {
            DisplayImage(imageInfo);
        }

        // 滚回顶部显示新内容
        Canvas.ForceUpdateCanvases();
        responseScroll.verticalNormalizedPosition = 1f;
    }

    private void DisplayImage(ImageInfo imageInfo)
    {
        GameObject imageItem = Instantiate(imageItemPrefab, contentContainer);
        displayedImages.Add(imageItem);

        var imageComponent = imageItem.GetComponent<Image>();
        var titleText = imageItem.transform.Find("Title").GetComponent<TextMeshProUGUI>();

        // 设置标题
        if (titleText != null)
        {
            titleText.text = imageInfo.title;
        }

        // 设置初始占位符颜色
        if (imageComponent != null)
        {
            imageComponent.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        }

        // 异步加载图片
        ImageLoader.Instance.LoadImage(imageInfo.imageUrl, (texture) =>
        {
            if (texture != null && imageComponent != null && imageItem != null)
            {
                imageComponent.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                imageComponent.color = Color.white;

                // 调整图片项目的高度，保持宽高比
                var layoutElement = imageItem.GetComponent<LayoutElement>();
                if (layoutElement != null)
                {
                    float aspectRatio = (float)texture.width / texture.height;
                    layoutElement.preferredHeight = layoutElement.preferredWidth > 0
                        ? layoutElement.preferredWidth / aspectRatio
                        : 200;
                }

                Debug.Log($"[DialoguePanel] 图片已显示: {imageInfo.title}");
            }
            else
            {
                Debug.LogWarning($"[DialoguePanel] 图片加载失败或已销毁: {imageInfo.imageUrl}");
            }
        });
    }

    private void ClearImages()
    {
        foreach (var image in displayedImages)
        {
            if (image != null)
                Destroy(image);
        }
        displayedImages.Clear();
    }

    private void CreateImageItemPrefab()
    {
        // 动态创建图片项目预制体
        GameObject imageItem = new GameObject("ImageItem");
        imageItem.SetActive(false);

        // 添加 LayoutElement，设置固定高度
        var layoutElement = imageItem.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 200;  // 图片高度 200
        layoutElement.preferredWidth = -1;    // 自动宽度（填充容器）

        // 添加 Image 组件显示图片
        var image = imageItem.AddComponent<Image>();
        image.raycastTarget = false;

        // 创建标题文本
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(imageItem.transform);
        titleObj.transform.localPosition = Vector3.zero;

        var titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "Image";
        titleText.fontSize = 20;
        titleText.alignment = TextAlignmentOptions.BottomLeft;

        var titleLayout = titleObj.AddComponent<LayoutElement>();
        titleLayout.preferredHeight = 40;

        imageItemPrefab = imageItem;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        EventCenter.Instance.RemoveListener<String>(EventName.AIChat, OnAIChat);
        EventCenter.Instance.RemoveListener<String>(EventName.LLMResponse, OnLLMResponse);
        EventCenter.Instance.RemoveListener<List<ImageInfo>>(EventName.LLMImages, OnLLMImages);
        ClearImages();
        if (imageItemPrefab != null)
            Destroy(imageItemPrefab);
    }
}
