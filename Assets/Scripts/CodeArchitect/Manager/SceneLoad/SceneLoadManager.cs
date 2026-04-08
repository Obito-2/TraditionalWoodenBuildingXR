using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System;

/// <summary>
/// �������ع���
/// </summary>
public class SceneLoadManager : SingletonBase<SceneLoadManager>
{
    /// <summary>
    /// �첽���س���
    /// </summary>
    /// <param name="sceneName"></param>
    /// <param name="onFinish"></param>
    /// <param name="resourceLoadWay"></param>
    public void LoadSceneAsync(string sceneName, Action onFinish, ResourceLoadWay resourceLoadWay=ResourceLoadWay.SceneManagement)
    {
        ClearUtil.ClearDataInManagers();
        if (resourceLoadWay == ResourceLoadWay.Addressables)
        {
            AddressablesManager.Instance.LoadSceneAsync(sceneName, ()=>
            {
                onFinish?.Invoke();
            });
        }
        else if(resourceLoadWay == ResourceLoadWay.SceneManagement)
        {
            MonoManager.Instance.StartCoroutine(I_LoadSceneAsync(sceneName, () =>
            {
                onFinish?.Invoke();
            }));
        }
    }
    private IEnumerator I_LoadSceneAsync(string name, Action onFinish)
    {
        AsyncOperation async = SceneManager.LoadSceneAsync(name);
        while (!async.isDone)
        {
            //�ַ����س����¼���һ����UI���س����Ķ���
            //���ﴥ��ʲô�¼���������������ִ���

            yield return async.progress;
        }
        onFinish?.Invoke();
        yield return null; 
    }
    
}

