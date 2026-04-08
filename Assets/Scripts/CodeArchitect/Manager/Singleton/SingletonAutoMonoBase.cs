using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonAutoMonoBase<T> : MonoBehaviour where T:MonoBehaviour
{
    private static bool isApplicationQuit;
    private static T instance;
    public static T Instance
    {
        get
        {
            if (isApplicationQuit)
            {
                return instance;
            }
            if (instance == null)
            {
                GameObject obj = new GameObject();
                obj.name = typeof(T).ToString();
                DontDestroyOnLoad(obj);//���д������ʵ����������Ƿ�д�ڻ�����
                instance = obj.AddComponent<T>();
            }
            return instance;
        }
    }
    protected virtual void OnDestroy()
    {
        isApplicationQuit = true;
    }
}
