using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleScriptableObject<T> : ScriptableObject where T : ScriptableObject
{
    //����������Դ�ļ�������Resources�ļ����¼��ض�Ӧ��������Դ�ļ�
    //����Ҫ���õ�Ψһ��������Դ�ļ�����һ�������ļ���������һ��
    private static string scriptableObjectPath = "06.Data/ScriptableObject/" + typeof(T).Name;
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                //���Ϊ�գ�����Ӧ��ȥ��Դ·���¼��ض�Ӧ��������Դ�ļ�
                instance = Resources.Load<T>(scriptableObjectPath);
            }
            //���û������ļ���ֱ�Ӵ���һ������
            if (instance == null)
            {
                instance = CreateInstance<T>();
            }
            return instance;
        }
    }
}

