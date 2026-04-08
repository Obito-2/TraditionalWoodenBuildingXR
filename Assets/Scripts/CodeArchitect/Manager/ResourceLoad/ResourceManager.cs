using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ResourceManager : SingletonBase<ResourceManager> 
{
	/// <summary>
	/// 同步加载资源（少用）
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="path"></param>
	/// <returns></returns>
	public T Load<T>(string path) where T: UnityEngine.Object
    {
		T resource = Resources.Load<T>(path);
		//如果对象是一个GameObject类型的 把它实例化后 再返回出去 外部 直接使用即可
		if (resource is GameObject)
			return GameObject.Instantiate(resource);
		else//TextAsset AudioClip
			return resource;

    }
	/// <summary>
	/// 异步加载资源 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="path"></param>
	/// <param name="callBack"></param>
	/// <param name="resourceLoadWay"></param>
	public void LoadAsync<T>(string path, Action<T> callBack, ResourceLoadWay resourceLoadWay = ResourceLoadWay.Addressables) where T : UnityEngine.Object
	{
		if (resourceLoadWay == ResourceLoadWay.Addressables)
		{
			AddressablesManager.Instance.LoadAssetAsync<T>(path, (res) =>
            {
				if (res is GameObject)
                {
					callBack?.Invoke(GameObject.Instantiate(res)) ;
				}
                else
                {
					callBack?.Invoke(res);
				}				
			}); 
		}
		else if (resourceLoadWay == ResourceLoadWay.Resources)
		{
			MonoManager.Instance.StartCoroutine(I_LoadResourceAsync<T>(path, callBack));
		}
	}
	//将异步的东西都放在协程里面做
	IEnumerator I_LoadResourceAsync<T>(string path, Action<T> callBack) where T : UnityEngine.Object
	{
		ResourceRequest request = Resources.LoadAsync<T>(path);
		//返回执行外部后面的代码回来后继续加载场景直到加载完毕往后执行
		yield return request;
		//加载完毕
		if (request.asset is GameObject)
		{			
			callBack(GameObject.Instantiate(request.asset) as T);
		}
		else callBack(request.asset as T);
	}
	

}

public enum ResourceLoadWay
{
	Addressables,
	Resources,
	SceneManagement
}
