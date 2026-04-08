using System;
using System.Collections;
using System.Collections.Generic;
using CodeArchitect.Manager.Event;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class DialoguePanel : BaseFadePanel
{
    private TextMeshProUGUI queryContent;
    private TextMeshProUGUI responseContent;
    private Button closeButton;

    protected override void Awake()
    {
        base.Awake();
        queryContent = transform.Find("queryContent").GetComponent<TextMeshProUGUI>();
        responseContent = transform.Find("responseContent").GetComponent<TextMeshProUGUI>();
        closeButton = transform.GetComponentInChildren<Button>();
        closeButton.onClick.AddListener(CloseDialoguePanel);
        
        //监听aiChat点击事件和模型response事件
        EventCenter.Instance.AddListener<String>(EventName.AIChat,UpdateUserQueryText);
        EventCenter.Instance.AddListener<String>(EventName.LLMResponse,UpdateResponseMessage);
    }
    
    private void CloseDialoguePanel()
    {
        //关闭面板
    }

    private void UpdateUserQueryText(String interactPanelModelInfo)
    {
        queryContent.text ="请介绍下八铺作补间铺作构件中的：" + interactPanelModelInfo;
        Debug.LogWarning("对话面板query更新成功");
    }

    private void UpdateResponseMessage(String responseMessage)
    {
        //TODO：根据响应状态展示文案，失败展示失败文案
        responseContent.text = responseMessage;
        Debug.LogWarning("对话面板response更新成功");
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        EventCenter.Instance.RemoveListener<String>(EventName.AIChat,UpdateUserQueryText);
        EventCenter.Instance.RemoveListener<String>(EventName.LLMResponse,UpdateResponseMessage);

    }
}
