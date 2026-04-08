using System;
using System.Collections;
using System.Collections.Generic;
using CodeArchitect.Manager.Event;
using UnityEngine;

/// <summary>
/// 事件中心 单例模式对象 （轻量级）
/// 1.Dictionary
/// 2.委托
/// 3.观察者设计模式
/// 4.泛型
/// </summary>
public class EventCenter : SingletonBase<EventCenter>
{
    //key —— 事件的名字（比如：怪物死亡，玩家死亡，通关 等等）
    //value —— 对应的是 监听这个事件 对应的委托函数们
    private Dictionary<EventName, List<Delegate>> eventDic = new Dictionary<EventName, List<Delegate>>();

    /// <summary>
    /// 监听事件
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="callback"></param>
    public void AddListenerBase(EventName eventName, Delegate callback)
    {
        if (eventDic.ContainsKey(eventName))
        {
            eventDic[eventName].Add(callback);
        }
        else
        {
            eventDic.Add(eventName, new List<Delegate>() { callback});
        }
    }
    /// <summary>
    /// 监听不需要参数传递的事件
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="callback"></param>
    public void AddListener(EventName eventName, Action callback)
    {
        AddListenerBase(eventName, callback);
    }
    /// <summary>
    /// 添加事件监听（1个参数）
    /// </summary>
    /// <param name="name">事件的名字</param>
    /// <param name="action">准备用来处理事件 的委托函数</param>
    public void AddListener<T>(EventName eventName, Action<T> callback)
    {
        AddListenerBase(eventName, callback);
    }
    /// <summary>
    /// 添加事件监听（2个参数）
    /// </summary>
    /// <param name="eventName">事件的名字</param>
    /// <param name="callback">准备用来处理事件 的委托函数</param>
    public void AddListener<T1, T2>(EventName eventName, Action<T1, T2> callback)
    {
        AddListenerBase(eventName, callback);
    }
    /// <summary>
    /// 添加事件监听（3个参数）
    /// </summary>
    /// <param name="eventName">事件的名字</param>
    /// <param name="callback">准备用来处理事件 的委托函数</param>
    public void AddListener<T1, T2, T3>(EventName eventName, Action<T1, T2, T3> callback)
    {
        AddListenerBase(eventName, callback);
    }
    /// <summary>
    /// 添加事件监听（4个参数）
    /// </summary>
    /// <param name="eventName">事件的名字</param>
    /// <param name="callback">准备用来处理事件 的委托函数</param>
    public void AddListener<T1, T2, T3, T4>(EventName eventName, Action<T1, T2, T3, T4> callback)
    {
        AddListenerBase(eventName, callback);
    }
    public void AddListenerWithReturnValue<R>(EventName eventName, Func<R> callback)
    {
        AddListenerBase(eventName, callback);
    }
    public void AddListenerWithReturnValue<T1, R>(EventName eventName, Func<T1, R> callback)
    {
        AddListenerBase(eventName, callback);
    }
    public void AddListenerWithReturnValue<T1,T2, R>(EventName eventName, Func<T1,T2, R> callback)
    {
        AddListenerBase(eventName, callback);
    }
    public void AddListenerWithReturnValue<T1, T2, T3, R>(EventName eventName, Func<T1, T2, T3, R> callback)
    {
        AddListenerBase(eventName, callback);
    }
    public void AddListenerWithReturnValue<T1, T2, T3, T4, R>(EventName eventName, Func<T1, T2, T3, T4, R> callback)
    {
        AddListenerBase(eventName, callback);
    }
    /// <summary>
    /// 移除事件
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="callback"></param>
    public void RemoveListenerBase(EventName eventName, Delegate callback)
    {
        if (eventDic.TryGetValue(eventName, out List<Delegate> eventList))
        {
            eventList.Remove(callback);
            //事件列表没有事件时，将该事件键值对从字典中移除
            if (eventList.Count == 0)
            {
                eventDic.Remove(eventName);
            }
        }
    }

    /// <summary>
    /// 移除不需要参数的事件
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="callback"></param>
    public void RemoveListener(EventName eventName, Action callback)
    {
        RemoveListenerBase(eventName, callback);
    }
    /// <summary>
    /// 移除对应的事件监听（1个参数）
    /// </summary>
    /// <param name="eventName">事件的名字</param>
    /// <param name="callback">对应之前添加的委托函数</param>
    public void RemoveListener<T>(EventName eventName, Action<T> callback)
    {
        RemoveListenerBase(eventName, callback);
    }
    /// <summary>
    /// 移除对应的事件监听（2个参数）
    /// </summary>
    /// <param name="eventName">事件的名字</param>
    /// <param name="callback">对应之前添加的委托函数</param>
    public void RemoveListener<T1, T2>(EventName eventName, Action<T1, T2> callback)
    {
        RemoveListenerBase(eventName, callback);
    }
    /// <summary>
    /// 移除对应的事件监听（3个参数）
    /// </summary>
    /// <param name="eventName">事件的名字</param>
    /// <param name="callback">对应之前添加的委托函数</param>
    public void RemoveListener<T1, T2, T3>(EventName eventName, Action<T1, T2, T3> callback)
    {
        RemoveListenerBase(eventName, callback);
    }
    /// <summary>
    /// 移除对应的事件监听（4个参数）
    /// </summary>
    /// <param name="eventName">事件的名字</param>
    /// <param name="callback">对应之前添加的委托函数</param>
    public void RemoveListener<T1, T2, T3, T4>(EventName eventName, Action<T1, T2, T3, T4> callback)
    {
        RemoveListenerBase(eventName, callback);
    }
    public void RemoveListenerWithReturnValue<R>(EventName eventName, Func<R> callback)
    {
        RemoveListenerBase(eventName, callback);
    }
    public void RemoveListenerWithReturnValue<T1, R>(EventName eventName, Func<T1, R> callback)
    {
        RemoveListenerBase(eventName, callback);
    }
    public void RemoveListenerWithReturnValue<T1, T2, R>(EventName eventName, Func<T1, T2, R> callback)
    {
        RemoveListenerBase(eventName, callback);
    }
    public void RemoveListenerWithReturnValue<T1, T2, T3, R>(EventName eventName, Func<T1, T2, T3, R> callback)
    {
        RemoveListenerBase(eventName, callback);
    }
    public void RemoveListenerWithReturnValue<T1, T2, T3, T4, R>(EventName eventName, Func<T1, T2, T3, T4, R> callback)
    {
        RemoveListenerBase(eventName, callback);
    }
    /// <summary>
    /// 事件触发（不需要参数的）
    /// </summary>
    /// <param name="eventName"></param>
    public void TriggerEvent(EventName eventName)
    {
        if (eventDic.ContainsKey(eventName))
        {
            foreach (Delegate callback in eventDic[eventName])
            {
                (callback as Action)?.Invoke();
            }
        }
    }
    /// <summary>
    /// 事件触发（1个参数）
    /// </summary>
    /// <param name="eventName">哪一个名字的事件触发了</param>
    public void TriggerEvent<T>(EventName eventName, T info)
    {
        if (eventDic.ContainsKey(eventName))
        {            
            foreach (Delegate callback in eventDic[eventName])
            {
                (callback as Action<T>)?.Invoke(info); 
            }
        }
    }
    /// <summary>
    /// 事件触发（2个参数）
    /// </summary>
    /// <param name="eventName">哪一个名字的事件触发了</param>
    public void TriggerEvent<T1, T2>(EventName eventName, T1 info1, T2 info2)
    {
        if (eventDic.ContainsKey(eventName))
        {
            foreach (Delegate callback in eventDic[eventName])
            {
                (callback as Action<T1, T2>)?.Invoke(info1, info2);
            }
        }
    }
    /// <summary>
    /// 事件触发（3个参数）
    /// </summary>
    /// <param name="eventName">哪一个名字的事件触发了</param>
    public void TriggerEvent<T1, T2, T3>(EventName eventName, T1 info1, T2 info2, T3 info3)
    {
        if (eventDic.ContainsKey(eventName))
        {
            foreach (Delegate callback in eventDic[eventName])
            {
                (callback as Action<T1, T2, T3>)?.Invoke(info1, info2, info3);
            }
        }
    }
    /// <summary>
    /// 事件触发（4个参数）
    /// </summary>
    /// <param name="eventName">哪一个名字的事件触发了</param>
    public void TriggerEvent<T1, T2, T3, T4>(EventName eventName, T1 info1, T2 info2, T3 info3, T4 info4)
    {
        if (eventDic.ContainsKey(eventName))
        {
            foreach (Delegate callback in eventDic[eventName])
            {
                (callback as Action<T1, T2, T3, T4>)?.Invoke(info1, info2, info3, info4);
            }
        }
    }
    public R TriggerEventWithReturnValue<R>(EventName eventName)
    {
        R value = default(R);
        if (eventDic.ContainsKey(eventName))
        {
            foreach (Delegate callback in eventDic[eventName])
            {
                if (callback is Func<R> funcCallback)
                {
                    value = funcCallback.Invoke();
                }
            }
            return value;
        }
        return value;
    }
    public R TriggerEventWithReturnValue<T1, R>(EventName eventName, T1 info1)
    {
        R value = default(R);
        if (eventDic.ContainsKey(eventName))
        {
            foreach (Delegate callback in eventDic[eventName])
            {
                if (callback is Func<T1, R> funcCallback)
                {
                    value = funcCallback.Invoke(info1);
                }
            }
            return value;
        }
        return value;
    }
    public R TriggerEventWithReturnValue<T1, T2, R>(EventName eventName, T1 info1, T2 info2)
    {
        R value = default(R);
        if (eventDic.ContainsKey(eventName))
        {
            foreach (Delegate callback in eventDic[eventName])
            {
                if (callback is Func<T1,T2, R> funcCallback)
                {
                    value = funcCallback.Invoke(info1, info2);
                }
            }
            return value;
        }
        return value;
    }
    public R TriggerEventWithReturnValue<T1, T2, T3, R>(EventName eventName, T1 info1, T2 info2, T3 info3)
    {
        R value = default(R);
        if (eventDic.ContainsKey(eventName))
        {
            foreach (Delegate callback in eventDic[eventName])
            {
                if (callback is Func<T1, T2, T3, R> funcCallback)
                {
                    value = funcCallback.Invoke(info1, info2, info3);
                }
            }
            return value;
        }
        return value;
    }
    public R TriggerEventWithReturnValue<T1, T2, T3, T4, R>(EventName eventName, T1 info1, T2 info2, T3 info3, T4 info4)
    {
        R value = default(R);
        if (eventDic.ContainsKey(eventName))
        {
            foreach (Delegate callback in eventDic[eventName])
            {
                if (callback is Func<T1, T2, T3, T4, R> funcCallback)
                {
                    value = funcCallback.Invoke(info1, info2, info3, info4);
                }
            }
            return value;
        }
        return value;
    }
    /// <summary>
    /// 清空事件中心
    /// 主要用在 场景切换时
    /// </summary>
    public void Clear()
    {
        eventDic.Clear();
    }
}
