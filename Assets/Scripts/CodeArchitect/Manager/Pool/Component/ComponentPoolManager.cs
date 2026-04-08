using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComponentPoolManager : SingletonBase<ComponentPoolManager>, IComponentPoolManager
{
    protected Dictionary<string, ComponentPool> poolDic = new Dictionary<string, ComponentPool>();

    public void RegularClearPoolObj()
    {
        throw new NotImplementedException();
    }

    public void ClearPool()
    {
        poolDic?.Clear();
    }

    /// <summary>
    /// �ӻ������ȡ�����
    /// </summary>
    /// <typeparam name="T">�������</typeparam>
    /// <param name="name">���ڱ�ʶ���������</param>
    /// <param name="obj">�����������Ϸ����</param>
    /// <param name="initCallback">��ʼ��������õ�ί��(����û�������</param>
    /// <param name="getFromPoolCallback">��һ���Ժ�ȡ������õ�ί��</param>
    public void GetItem<T>(string name, Action initCallback, Action<T> getFromPoolCallback) where T : Component
    {
        if (poolDic.ContainsKey(name) && poolDic[name].itemPool.Count > 0)
        {
            getFromPoolCallback?.Invoke(poolDic[name].GetItem() as T);
        }
        else
        {
            initCallback?.Invoke();
        }       
    }

    public bool IsItemInPool(string keyName)
    {
        if (poolDic.ContainsKey(keyName))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void PushItem(string name, Component component)
    {
        if(poolDic.TryGetValue(name, out var pool))
        {
            pool.PushItem(component);
        }
        else
        {
            poolDic.Add(name, new ComponentPool(component));
        }
    }

    public void RemovePoolItem(string name)
    {
        if (poolDic.ContainsKey(name))
        {
            poolDic.Remove(name);
        }
    }
}
