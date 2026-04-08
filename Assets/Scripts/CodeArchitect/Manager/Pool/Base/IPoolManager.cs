using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPoolManager
{
    public void ClearPool();
    public void RemovePoolItem(string name);
    public bool IsItemInPool(string keyName);
    public  void RegularClearPoolObj();
}
public interface IGameObjectPoolManager : IPoolManager
{
    public void GetItem(string resName, Action<GameObject> callback, string keyName = null, Action<GameObject> callbackIfInPool = null, ResourceLoadWay resourceLoadWay = ResourceLoadWay.Addressables);
    public void PushItem(string name, GameObject obj);
}
public interface IComponentPoolManager : IPoolManager
{
    public void GetItem<T>(string name, Action initCallback, Action<T> getFromPoolCallback) where T : Component;
    public void PushItem(string name, Component component);
}

