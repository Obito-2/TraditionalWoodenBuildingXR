using System;
using System.Collections;
using System.Collections.Generic;
using CodeArchitect.Manager.Event;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class LLMChat : MonoBehaviour
{
    private string apiKey;
    private string apiUrl;
    private string modelName;
    //系统提示词
    [SerializeField]
    public SystemPrompt systemPrompt;

    private void Awake()
    {
        apiKey = LlmEnv.ApiKey;
        apiUrl = LlmEnv.ApiUrl;
        modelName = LlmEnv.Model;
        if (string.IsNullOrEmpty(apiKey))
            Debug.LogError("未在 .env 中配置 LLM_API_KEY，无法调用 LLM。");
        if (string.IsNullOrEmpty(apiUrl))
            Debug.LogError("未在 .env 中配置 LLM_API_URL，无法调用 LLM。");
        if (string.IsNullOrEmpty(modelName))
            Debug.LogError("未在 .env 中配置 LLM_MODEL，无法调用 LLM。");

        //订阅aichat按钮点击事件，获取query，触发发送请求协程
        EventCenter.Instance.AddListener<String>(EventName.AIChat,SendQueryToLLM);
    }

    private void SendQueryToLLM(String interactPanelModelInfo)
    {
        //启动协程方法发送请求，协程完成后将请求状态、结果数据发送给dialouguePanel做展示
        StartCoroutine(PostRequest("请介绍下八铺作补间铺作构件中的：" + interactPanelModelInfo, (response) =>
        {
            EventCenter.Instance.TriggerEvent(EventName.LLMResponse, response);

        }));
    }

    /// <summary>
    /// 协程方法，组织参数并发送请求、处理响应
    /// </summary>
    /// <param name="message"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    
    IEnumerator PostRequest(string message, UnityAction<string> callback)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            callback?.Invoke("出错了: 未在 .env 中配置 LLM_API_KEY");
            yield break;
        }
        if (string.IsNullOrEmpty(apiUrl))
        {
            callback?.Invoke("出错了: 未在 .env 中配置 LLM_API_URL");
            yield break;
        }
        if (string.IsNullOrEmpty(modelName))
        {
            callback?.Invoke("出错了: 未在 .env 中配置 LLM_MODEL");
            yield break;
        }

        List<Message> messages = new List<Message>
        {
            new Message { role = "system", content = systemPrompt.prompt },//系统角色设定
            new Message { role = "user",content = message}//用户输入
        };
        //组装请求体
        ChatRequest requestBody = new ChatRequest
        {
            model = modelName,
            messages = messages
        };
        //序列化为json
        string jsonBody = JsonUtility.ToJson(requestBody);
        Debug.LogWarning(jsonBody);
        
        //创建unityRequest，发送请求
        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);
        
        yield return request.SendWebRequest();//暂停协程，等网络返回结果，再恢复执行。
        
        //判断请求是否成功，处理结果
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error: " + request.error);
            Debug.LogError("Response: " + request.downloadHandler.text);
            callback?.Invoke("出错了: " + request.error);
        }
        else
        {
            string responseJson = request.downloadHandler.text;
            Debug.Log("Response JSON: " + responseJson);

            string responseMessage = GetResponseMessages(responseJson);
            callback?.Invoke(responseMessage);//如果调用方传了回调函数，就执行它，把response信息当作参数传进去
        }
    }
    
    /// <summary>
    /// 解析模型response返回，并从choice字段中提取message进行返回
    /// </summary>
    /// <param name="responseJson"></param>
    /// <returns></returns>
    public string GetResponseMessages(string responseJson)
    {
        string responseMessage = "";
        ResponseData responseData = JsonUtility.FromJson<ResponseData>(responseJson);
        if (responseData != null && responseData.choices.Length > 0)
        {
            responseMessage = responseData.choices[0].message.content;  //提取消息文本
            Debug.LogWarning("ResponseMessage: "+ responseMessage);
        }
        return responseMessage;
    }
    //组装请求参数的类
    [System.Serializable]
    private class ChatRequest
    {
        public string model;
        public List<Message> messages;

    }
    //消息类
    [System.Serializable]
    public class Message
    {
        public string role;
        public string content;
    }
    //prompt类
    [System.Serializable]
    public class SystemPrompt
    {
        public string name = "";
        [TextArea(1, 100)] public string prompt = "";//角色设定prompt
    }
    // 用来解析 API 响应的 C# 类
    [System.Serializable]
    public class ResponseData
    {
        public Choice[] choices;
    }
    //根据官网文档，包含消息content的类
    [System.Serializable]
    public class Choice
    {
        public Message message;
    }

    private void OnDestroy()
    {
        EventCenter.Instance.RemoveListener<String>(EventName.AIChat, SendQueryToLLM);
    }
}
