using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
/// <summary>
/// UI淡入淡出的面板基类
/// </summary>
public class BaseFadePanel : BaseUIPanel
{
    protected CanvasGroup canvasGroup;
    [SerializeField]
    protected float alphaShowSpeed = 2;  //淡入速度
    [SerializeField]
    protected float alphaHideSpeed = 2;  //淡出速度
    [SerializeField]
    private AudioSource buttonClickedAudio;
    protected bool isShow = false;
    private Action hideCallBack;  //面板透明度为0时调用的委托
    private Action showCallBack;   //面板透明度为1时调用的委托
    private Action beginCallback;
    protected override void Awake()
    {
        base.Awake(); 
        //一开始获取面板上挂载的组件
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>(); 
        }
    }
    protected override void Update()
    {
        if (isShow && canvasGroup.alpha != 1)
        {
            canvasGroup.alpha += Time.deltaTime * alphaShowSpeed;
            if (canvasGroup.alpha >= 1)
            {
                canvasGroup.alpha = 1;
                SetInteractableOfCanvasGroup(true);
                showCallBack?.Invoke(); 
            }

        }
        else if (!isShow && canvasGroup.alpha != 0)
        {
            canvasGroup.alpha -= Time.deltaTime * alphaShowSpeed;
            if (canvasGroup.alpha <= 0)
            {
                canvasGroup.alpha = 0;
                hideCallBack?.Invoke();
                if (gameObject.activeSelf)
                {
                    gameObject.SetActive(false);
                }
            }
        }
    }
    protected override void OnClick(Button button)
    {
        base.OnClick(button);
        if(buttonClickedAudio != null)
        {
            buttonClickedAudio?.Play();
        }
        
    }
    protected void SetInteractableOfCanvasGroup(bool isEnbale)
    {
        if (canvasGroup != null)
        {
            canvasGroup.interactable = isEnbale;
        }
    }
    
    /// <summary>
    /// 显示自己，淡入淡出模式
    /// </summary>
    /// <param name="onFinish">UI透明度由0变化到1时调用的委托</param>
    public override void Show(Action onFinish = null, Action onBegin = null)
    {
        isShow = true;
        beginCallback = onBegin;
        beginCallback?.Invoke();
        showCallBack = onFinish;
        canvasGroup.alpha = 0;
        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
        }
    }
    /// <summary>
    /// 隐藏自己，淡入淡出模式
    /// </summary>
    /// <param name="onFinish">UI透明时调用的委托</param>
    public override void Hide(Action onFinish = null, Action onBegin = null)
    {
        isShow = false;
        beginCallback = onBegin;
        beginCallback?.Invoke();
        hideCallBack = onFinish;
        canvasGroup.alpha = 1;
        SetInteractableOfCanvasGroup(false);
    }
}
