using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 通过调用 Mono 所拥有的 MonoBehaviour 为外部提供 Update 、协程等
/// </summary>
public class MonoManager : SingletonBase<MonoManager>
{
    private MonoHandler mono;
    public MonoManager()
    {
        mono = new GameObject("MonoHolder").AddComponent<MonoHandler>();
    }
    /// <summary>
    /// 添加 Update 期间执行的 Action
    /// </summary>
    /// <param name="action">添加的 Action</param>
    public void AddUpdateAction(UnityAction action)
    {
         mono?.AddUpdateEvent(action);
    }

    /// <summary>
    /// 移除 Update 期间执行的 Action
    /// </summary>
    /// <param name="action">移除的 Action</param>
    public void RemoveUpdateAction(UnityAction action)
    {
          mono?.RemoveUpdateEvent(action);
    }

    /// <summary>
    /// 使用指定的方法名启动一个协程
    /// </summary>
    /// <param name="methodName">方法名</param>
    /// <returns>协程对象</returns>
    public Coroutine StartCoroutine(string methodName)
    {        
          return mono?.StartCoroutine(methodName);
    }

    /// <summary>
    /// 使用指定的迭代器启动一个协程
    /// </summary>
    /// <param name="routine">迭代器</param>
    /// <returns>协程对象</returns>
    public Coroutine StartCoroutine(IEnumerator routine)
    {
         return mono?.StartCoroutine(routine);
    }

    /// <summary>
    /// 停止指定迭代器对应的协程
    /// </summary>
    /// <param name="routine">迭代器</param>
    public void StopCoroutine(IEnumerator routine)
    {
          mono?.StopCoroutine(routine);
    }

   
}