using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameObjectPoolManager : SingletonBase<GameObjectPoolManager>, IGameObjectPoolManager
{
    protected Dictionary<string, GameObjectPool> poolDic = new Dictionary<string, GameObjectPool>();
    private GameObject rootObj;
    private PoolClearSetting poolClearSetting=new PoolClearSetting();


    /// <summary>
    /// �ӻ������ȡ������
    /// </summary>
    /// <param name="resName"></param>
    /// <param name="callback"></param>
    /// <param name="resourceLoadWay"></param>
    public void GetItem(string resName, Action<GameObject> callback, string keyName = null, Action<GameObject> callbackIfInPool = null, ResourceLoadWay resourceLoadWay = ResourceLoadWay.Addressables)
    {
        if(keyName == null)
        {
            keyName = resName;
        }
        if(callbackIfInPool == null)
        {
            callbackIfInPool = callback;
        }
        if(poolDic.ContainsKey(keyName) && poolDic[keyName].itemPool.Count > 0)
        {
            callbackIfInPool?.Invoke(poolDic[keyName].GetItem());
        }
        else
        {
            ResourceManager.Instance.LoadAsync<GameObject>(resName, (obj) =>
            {
                obj.name = resName;
                callback?.Invoke(obj);
            }, resourceLoadWay);

        }
    }
    /// <summary>
    /// ����ʱ���õ���Ϸ����Żػ����
    /// </summary>
    /// <param name="name"></param>
    /// <param name="obj"></param>
    public void PushItem(string name, GameObject obj)
    {
        if (rootObj == null)
        {
            rootObj = new GameObject("Pool");
            Scheduler.Instance.AddTimerTask(poolClearSetting.ClearObjInterval, RegularClearPoolObj);

        }
        if(poolDic.TryGetValue(name, out var pool))
        {
            pool.PushItem(obj);
        }
        else
        {
            poolDic.Add(name, new GameObjectPool(obj, rootObj));
        }
    }
    public void ClearPool()
    {
        poolDic?.Clear();
        rootObj = null;
        Scheduler.Instance.Remove(RegularClearPoolObj);
    }
    public void RemovePoolItem(string name)
    {
        if (poolDic.ContainsKey(name))
        {
            poolDic.Remove(name);
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

    public virtual void RegularClearPoolObj()
    {
        /*
        for (int i = 0; i < poolDic.Count; i++)
        {
            var keyValuePair = poolDic.ElementAt(i);
            GameObjectPool pool = keyValuePair.Value;
            //if (pool.itemPool.Count == 0) continue; 
            for(int j = 0; j < pool.itemPool.Count; j++)
            {
                GameObject poolObj = pool.itemPool[j];
                if (!poolObj.activeSelf)
                {
                    pool.itemPool.DeleteAt(j);
                    UnityEngine.Object.Destroy(poolObj);
                }
            }
            if (pool.fatherObj.transform.childCount == 0 || pool.itemPool.Count == 0)
            {
                RemovePoolItem(keyValuePair.Key);
                UnityEngine.Object.Destroy(pool.fatherObj);
            }
        }
        */
    }
    

    
}
