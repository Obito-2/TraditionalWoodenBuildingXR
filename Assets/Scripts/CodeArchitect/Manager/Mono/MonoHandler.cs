using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 脚本组件，其在 Update 期间引发一个事件
/// </summary>
public class MonoHandler : MonoBehaviour
{
    private event UnityAction UpdateEvent;
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
    private void Update()
    {
        UpdateEvent?.Invoke();
    }

    public static MonoHandler operator+ (MonoHandler mono, UnityAction action)
    {
        mono.AddUpdateEvent(action);
        return mono;
    }

    public static MonoHandler operator- (MonoHandler mono, UnityAction action)
    {
        mono.RemoveUpdateEvent(action);
        return mono;
    }

    /// <summary>
    /// 添加 UpdateEvent 的处理函数
    /// </summary>
    /// <param name="action">函数委托</param>
    public void AddUpdateEvent(UnityAction action) => UpdateEvent += action;

    /// <summary>
    /// 移除 UpdateEvent 的处理函数
    /// </summary>
    /// <param name="action">函数委托</param>
    public void RemoveUpdateEvent(UnityAction action) => UpdateEvent -= action;
}