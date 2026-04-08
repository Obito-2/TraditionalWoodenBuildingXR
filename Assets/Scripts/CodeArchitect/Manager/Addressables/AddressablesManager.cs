using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressablesManager : SingletonBase<AddressablesManager>
{
    //IEnumerator为AsyncOperationHandle<TObject>的基接口
    private Dictionary<string, IEnumerator> resDic = new Dictionary<string, IEnumerator>();
    /// <summary>
    /// 加载资源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <param name="onSuccess">Action<AsyncOperationHandle<T>>类型的委托，加载成功时调用</param>
    /// <param name="onFailure">加载失败时调用的委托</param>
    public void LoadAssetAsync<T>(string name, Action<AsyncOperationHandle<T>> onSuccess, Action onFailure=null)
    {
        //区分相同名字的不同类型的资源
        string keyName = name + "_" + typeof(T).Name;
        AsyncOperationHandle<T> handle;
        if (resDic.ContainsKey(keyName))
        {
            //as 运算符必须跟引用类型或者可以为null的类型，而AsyncOperationHandle<T>是结构体，所以不能用as转换
            //获取异步加载返回的内容
            handle = (AsyncOperationHandle<T>)resDic[keyName];
            //判断异步加载是否结束，因为有可能连续调用，第二次调用时可能前一次异步加载还未完成，导致handle不是完全的
            if (handle.IsDone)
            {
                onSuccess?.Invoke(handle); //已经加载完成了就手动调用委托
            }
            else
            {
                //这个时候如果还没有异步加载完成，就告诉它完成时需要做什么
                handle.Completed += (obj) => {
                    if (obj.Status == AsyncOperationStatus.Succeeded)
                    {
                        onSuccess?.Invoke(obj);
                    }
                };
            }
            return;
        }
        //如果没有加载过该资源，就直接异步加载，并存入字典
        handle = Addressables.LoadAssetAsync<T>(name);
        handle.Completed += (obj) => {
            if (obj.Status == AsyncOperationStatus.Succeeded)
            {
                onSuccess?.Invoke(obj);
            }
            //只有第一次加载资源失败时需要从字典中移除
            else
            {
                Debug.LogWarning(keyName + "资源加载失败");
                onFailure?.Invoke();
                if (resDic.ContainsKey(keyName))
                    resDic.Remove(keyName);
            }
        };
        resDic.Add(keyName, handle);
    }
    /// <summary>
    /// 加载资源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <param name="onSuccess">Action<T>类型的委托，绑定的方法直接以T为参数</param>
    /// <param name="onFailure">加载失败时调用的委托</param>
    public void LoadAssetAsync<T>(string name, Action<T> onSuccess, Action onFailure=null)
    {
        //区分相同名字的不同类型的资源
        string keyName = name + "_" + typeof(T).Name;
        AsyncOperationHandle<T> handle;
        if (resDic.ContainsKey(keyName))
        {
            //as 运算符必须跟引用类型或者可以为null的类型，而AsyncOperationHandle<T>是结构体，所以不能用as转换
            //获取异步加载返回的内容
            handle = (AsyncOperationHandle<T>)resDic[keyName];
            //判断异步加载是否结束，因为有可能连续调用，第二次调用时可能前一次异步加载还未完成，导致handle不是完全的
            if (handle.IsDone)
            {
                onSuccess?.Invoke(handle.Result); //已经加载完成了就手动调用委托
            }
            else
            {
                //这个时候如果还没有异步加载完成，就告诉它完成时需要做什么
                handle.Completed += (obj) => {
                    if (obj.Status == AsyncOperationStatus.Succeeded)
                    {
                        onSuccess?.Invoke(obj.Result);
                    }
                };
            }
            return;
        }
        //如果没有加载过该资源，就直接异步加载，并存入字典
        handle = Addressables.LoadAssetAsync<T>(name);
        handle.Completed += (obj) => {
            if (obj.Status == AsyncOperationStatus.Succeeded)
            {
                onSuccess?.Invoke(obj.Result);
            }
            //只有第一次加载资源失败时需要从字典中移除
            else
            {
                Debug.LogWarning(keyName + "资源加载失败");
                onFailure?.Invoke();
                if (resDic.ContainsKey(keyName))
                    resDic.Remove(keyName);
            }
        };
        resDic.Add(keyName, handle);
    }
    /// <summary>
    /// 异步加载多个资源或加载指定资源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="mode"></param>
    /// <param name="onSuccess"></param>
    /// <param name="onFailure">加载失败时调用的委托</param>
    /// <param name="key"></param>
    public void LoadAssetAsync<T>(Addressables.MergeMode mode, Action<T> onSuccess, Action onFailure, params string[] keys)
    {
        AsyncOperationHandle<IList<T>> handle;
        //1.构建一个keyName，用于存入字典中
        List<string> list = new List<string>(keys); //因为Addressables提供的方法需要传List，所以这里进行转化
        string keyName = GetKeyNameFromKeys(keys, typeof(T).Name); ;
        //2.判断是否存在加载过的资源
        if (resDic.ContainsKey(keyName))
        {
            handle = (AsyncOperationHandle<IList<T>>)resDic[keyName];
            if (handle.IsDone)
            {
                foreach (T item in handle.Result)
                {
                    onSuccess?.Invoke(item);
                }
            }
            else
            {
                //加载成功才调用传入的回调
                //这里判断条件是成功因为第一次加载资源会调用Addressables.LoadAssetsAsync，加载成功时会自动调用回调
                //而代码跑到这说明不是第一次调用，此时我们要手动调用回调，就必须在加载成功的条件下
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    foreach (T item in handle.Result)
                    {
                        onSuccess?.Invoke(item);
                    }
                }
            }
            return;
        }
        handle = Addressables.LoadAssetsAsync(list, onSuccess, mode);
        handle.Completed += (obj) =>
        {
            //这里只用处理加载失败，因为如果加载成功就会自动调用加载资源API所传入的回调
            if (obj.Status == AsyncOperationStatus.Failed)
            {
                Debug.LogWarning(keyName + "资源加载失败");
                onFailure?.Invoke();
                if (resDic.ContainsKey(keyName))
                    resDic.Remove(keyName);
            }
        };
        resDic.Add(keyName, handle);
    }
    /// <summary>
    /// 异步加载场景
    /// </summary>
    /// <param name="sceneName">场景名</param>
    /// <param name="onFinish">场景加载完毕后的委托</param>
    /// <param name="onFailure">加载失败时调用的委托</param>
    public void LoadSceneAsync(string sceneName, Action onFinish, Action onFailure=null)
    {
        var handle= Addressables.LoadSceneAsync(sceneName);
        handle.Completed += (obj) =>
        {
            if(handle.Status == AsyncOperationStatus.Succeeded)
            {
                onFinish?.Invoke();
                Addressables.Release(handle);
            }
            else
            {
                Debug.LogWarning($"{sceneName}场景加载失败");
                onFailure?.Invoke();
            }
            
        };
    }
    /// <summary>
    /// 异步获取资源大小
    /// </summary>
    /// <param name="name"></param>
    /// <param name="onSuccess"></param>
    public void GetDownloadSizeAsync(string name, Action<float> onSuccess, Action onFailure=null)
    {
        MonoManager.Instance.StartCoroutine(I_GetDownLoadSizeAsync(name, onSuccess));
    }
    IEnumerator I_GetDownLoadSizeAsync(string name, Action<float> onSuccess, Action onFailure=null)
    {
        //首先获取下载包的大小
        var handleSize = Addressables.GetDownloadSizeAsync(name); //单位是B
        yield return handleSize;
        if (handleSize.Result > 0)
        {
            onSuccess?.Invoke(handleSize.Result);
            Addressables.Release(handleSize);
        }
        else
        {
            onFailure?.Invoke();
        }
    }
    /// <summary>
    /// 异步下载资源
    /// </summary>
    /// <param name="name"></param>
    /// <param name="onFinish"></param>
    /// <param name="onProgress"></param>
    public void PreDownloadAsync(string name, Action onFinish, Action<DownloadStatus> onProgress)
    {
        MonoManager.Instance.StartCoroutine(I_PreDownloadAsync(name,onFinish,onProgress));
    }
    IEnumerator I_PreDownloadAsync(string name, Action onFinish, Action<DownloadStatus> onProgress)
    {
        var handle = Addressables.DownloadDependenciesAsync(name);
        while (!handle.IsDone)
        {
            //加载进度
            onProgress?.Invoke(handle.GetDownloadStatus());
            yield return 0;
        }
        onFinish?.Invoke();
        yield return 0;
       Addressables.Release(handle);
    }
    /// <summary>
    /// 释放资源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    public void Release<T>(string name)
    {
        string keyName = name + "_" + typeof(T).Name;
        if (resDic.ContainsKey(keyName))
        {
            Addressables.Release((AsyncOperationHandle<T>)resDic[keyName]);
            resDic.Remove(keyName);
        }
    }
    /// <summary>
    /// 释放多个资源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="keys"></param>
    public void Release<T>(params string[] keys)
    {
        string keyName = GetKeyNameFromKeys(keys, typeof(T).Name);
        if (resDic.ContainsKey(keyName))
        {
            Addressables.Release((AsyncOperationHandle<IList<T>>)resDic[keyName]);
            resDic.Remove(keyName);
        }
    }
    /// <summary>
    /// 清空资源
    /// </summary>
    public void Clear()
    {
        resDic.Clear();
        AssetBundle.UnloadAllAssetBundles(true);
        Resources.UnloadUnusedAssets();
        GC.Collect();
    }
    /// <summary>
    /// 根据多个key的名字和资源类型名拼接成一个存储在字典中的keyName
    /// </summary>
    /// <param name="keys"></param>
    /// <param name="typeName"></param>
    /// <returns></returns>
    private string GetKeyNameFromKeys(string[] keys, string typeName)
    {
        List<string> list = new List<string>(keys); //因为Addressables提供的方法需要传List，所以这里进行转化
        string keyName = string.Empty;
        foreach (var key in list)
        {
            keyName += key + "_";
        }
        keyName += typeName;
        return keyName;
    }

}


