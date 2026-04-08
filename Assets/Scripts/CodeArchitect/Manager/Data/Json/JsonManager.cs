using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using System;

/// <summary>
/// ���л��ͷ����л�Jsonʱ  ʹ�õ������ַ���
/// LitJson��Newtonsoft������Ҫ.NET 4.x
/// </summary>
public enum JsonType
{
    JsonUtlity,
    LitJson,
    NewtonsoftJson
}

/// <summary>
/// Json���ݹ����� ��Ҫ���ڽ��� Json�����л��洢��Ӳ�� �� �����л���Ӳ���ж�ȡ���ڴ���
/// </summary>
public class JsonManager : SingletonBase<JsonManager>
{
    /// <summary>
    /// �洢Json���� ���л�
    /// ����ת����Json�ַ���д�뵽�ļ���
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <param name="fileName"></param>
    /// <param name="type"></param>
    public void SaveData<T>(T data, string fileName, bool isSaveInPersistentDataPath = true, JsonType type = JsonType.NewtonsoftJson) where T : new()
    {
        string path = null;
        //ȷ���洢·��
        if (isSaveInPersistentDataPath)
        {
            path = Application.persistentDataPath + "/" + fileName + ".json";
        }
        else
        {
            path = Application.streamingAssetsPath + "/" + fileName + ".json";
        }
        //���л� �õ�Json�ַ���
        string jsonStr = "";
        switch (type)
        {
            case JsonType.JsonUtlity:
                jsonStr = JsonUtility.ToJson(data);
                break;
            case JsonType.LitJson:
                //jsonStr = JsonMapper.ToJson(data);
                break;
            case JsonType.NewtonsoftJson:
                jsonStr = JsonConvert.SerializeObject(data); Debug.Log(jsonStr);
                break;
        }
        //�����л���Json�ַ��� �洢��ָ��·�����ļ���
        File.WriteAllText(path, jsonStr);
    }

    /// <summary>
    /// ��ȡָ���ļ��е� Json���� �����л�
    /// �ҵ�Json�ļ������ı�ת��Ϊ����
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="fileName"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public T LoadData<T>(string fileName, bool isSaveInPersistentDataPath = true, JsonType type = JsonType.NewtonsoftJson) where T : new()
    {
        string jsonStr = null;
        string path = null;
        //ȷ���洢·��
        if (isSaveInPersistentDataPath)
        {
            path = Application.persistentDataPath + "/" + fileName + ".json";
        }
        else
        {
            path = Application.streamingAssetsPath + "/" + fileName + ".json";
        }
        //�����д�ļ����ж���û�� �Ǿͷ���һ��Ĭ�϶���
        if (!File.Exists(path))
        {
            return new T();
        }
        //���з����л�

        jsonStr = File.ReadAllText(path);
        //���ݶ���
        T data = default(T); //����T��Ĭ��ֵ
        switch (type)
        {
            case JsonType.JsonUtlity:
                data = JsonUtility.FromJson<T>(jsonStr);
                break;
            case JsonType.LitJson:
                //data = JsonMapper.ToObject<T>(jsonStr);
                break;
            case JsonType.NewtonsoftJson:
                data = JsonConvert.DeserializeObject<T>(jsonStr);
                break;
        }
        //�Ѷ��󷵻س�ȥ
        return data;
    }
    private T JsonToObj<T>(string jsonStr, JsonType type = JsonType.NewtonsoftJson) where T:new()
    {
        T data = default(T); //����T��Ĭ��ֵ
        switch (type)
        {
            case JsonType.JsonUtlity:
                data = JsonUtility.FromJson<T>(jsonStr);
                break;
            case JsonType.LitJson:
                //data = JsonMapper.ToObject<T>(jsonStr);
                break;
            case JsonType.NewtonsoftJson:
                data = JsonConvert.DeserializeObject<T>(jsonStr);
                break;
        }
        //�Ѷ��󷵻س�ȥ
        return data;
    }
    public void LoadDataFromAB<T>(string fileName,Action<T> callback, JsonType type = JsonType.NewtonsoftJson) where T : new()
    {
        ResourceManager.Instance.LoadAsync<TextAsset>(fileName, (textAsset) =>
        {
            T data = JsonToObj<T>(textAsset.text, type);
            callback?.Invoke(data);
        });
    }
    public void DeleteData(string fileName)
    {
        //ȷ���洢·��
        string path = Application.persistentDataPath + "/" + fileName + ".json";
        if (!File.Exists(path))
        {
            Debug.Log($"�Ҳ���{path}����δ����");
            return;
        }
        File.Delete(path);
    }
}

