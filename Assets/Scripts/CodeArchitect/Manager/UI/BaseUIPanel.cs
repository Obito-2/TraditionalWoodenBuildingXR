using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.Events;

/// <summary>
/// 面板基类
/// 方便通过代码快速找到自己面板下所有的UI控件对象(脚本)，然后在子类中实现相关逻辑
/// 提供显示、隐藏接口
/// </summary>
public class BaseUIPanel : BasePanel
{
    /// <summary>
    /// 字典记录UI物体带有的所有UI组件(所有的UI组件均继承于UIBehaviour),string为gameObject名字
    /// </summary>
    private Dictionary<string, List<UIBehaviour>> uiDic = new Dictionary<string, List<UIBehaviour>>();
    protected virtual void Awake()
    {
        //将 Panel 下所有UI组件添加到字典中
        //示例用法，项目中没有出现的组件可以不用寻找，也可以在子类的Awake里添加
        AddChildrenUIComponentsToDic<Button>();
        AddChildrenUIComponentsToDic<TextMeshProUGUI>();
        AddChildrenUIComponentsToDic<Image>();
        AddChildrenUIComponentsToDic<Toggle>();
        AddChildrenUIComponentsToDic<Slider>();
        AddChildrenUIComponentsToDic<ScrollRect>();
        AddChildrenUIComponentsToDic<TMP_InputField>();
    }                                                                                                                                                        
    protected virtual void OnDestroy()
    {
        ClearDic();
    }
    protected virtual void Update() { }
    
    /// <summary>
    /// 按钮触发的虚方法，子类中需重写此方法添加具体的处理逻辑
    /// </summary>
    /// <param name="buttonObjName">按钮对象名（一个面板下可能有多个按钮）</param>
    /// <param name="button">按钮组件引用</param>
    protected virtual void OnClick(Button button)
    {
        
    }
    /// <summary>
    /// Toggle 触发的虚方法
    /// </summary>
    /// <param name="toggleObjName">Toggle 对象名（一个面板下可能有多个 Toggle）</param>
    /// <param name="value">Toggle 值</param>
    /// <param name="toggle">Toggle 组件引用</param>
    protected virtual void OnToggleValueChanged(bool value, Toggle toggle) { }
    /// <summary>
    /// Slider 触发的虚方法
    /// </summary>
    /// <param name="value">Slider 值</param>
    /// <param name="slider">Slider 组件引用</param>
    protected virtual void OnSliderValueChanged(float value,Slider slider) { }
    /// <summary>
    /// 获取Panel子物体中所有T类型的组件，添加到字典中，免去添加控件引用或拖动控件等繁琐操作
    /// </summary>
    /// <typeparam name="T"></typeparam>
    protected void AddChildrenUIComponentsToDic<T>() where T: UIBehaviour
    {        
        if (uiDic is null)
            return;

        var components = GetComponentsInChildren<T>(true); //获取Panel下的所有T类型的子组件 
        foreach (var uiComponent in components)
        {
            var objName = uiComponent.gameObject.name;
            if (!uiDic.ContainsKey(objName))
                uiDic.Add(objName, new List<UIBehaviour> { uiComponent });
            else
                uiDic[objName].Add(uiComponent);

            switch (uiComponent)
            {
                
                //如果组件是按钮，直接添加监听事件
                //这样如果是一个按钮游戏物体只需要挂一个PanelBase的子脚本，重写OnClick方法，点击后就自动触发点击事件
                case Button buttonComponent:
                    //AddListener只能传无参委托，所以用Lambda表达式包一层
                    buttonComponent.onClick.AddListener(() =>
                    {
                        OnClick(buttonComponent);                    
                    });
                    break;
                
                //如果组件是单选框或多选框
                case Toggle toggleComponent:
                    toggleComponent.onValueChanged.AddListener((value) =>
                    {
                        OnToggleValueChanged(value, toggleComponent);                       
                    });
                    break;
                case Slider sliderComponent:
                    sliderComponent.onValueChanged.AddListener((value) =>
                    {
                        OnSliderValueChanged(value, sliderComponent);
                    });
                    break;
            }
        }
    }
    /// <summary>
    /// 获取对应名字的对应控件脚本
    /// PanelBase的子类就能在Start方法中调用此方法获取对应UI脚本
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns></returns>
    public T GetUIComponentByName<T>(string name) where T: UIBehaviour
    {
        if (!uiDic.ContainsKey(name))
        {
            return null;
        }
            

        foreach (var uiBehaviour in uiDic[name])
        {
            if (uiBehaviour is T castBehaviour)
                return castBehaviour;
        }
        return null;
    }

    public void ClearDic()
    {
        uiDic?.Clear();
        UI3DManager.Instance.RemovePanelFromPanelDic(name); 
    }
    protected void DestroySelf(Action onFinish = null, Action onBegin = null)
    {
        UI3DManager.Instance.DestroyPanel(gameObject.name, () =>
        {
            onFinish?.Invoke();
        }, onBegin);
    }
    protected void HideSelf(Action onFinish = null, Action onBegin = null, bool needSavePanel = false)
    {
        UI3DManager.Instance.HidePanel(gameObject.name, () =>
        {
            onFinish?.Invoke();
        }, onBegin, needSavePanel);
    }
}
