using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class llm_test : MonoBehaviour
{
    private string apiKey = "sk-c5e2dbdbf1e04d0c9fc98b2420935a3e";
    private string apiUrl = "https://api.deepseek.com/chat/completions";
    // Start is called before the first frame update
    void Start()
    {
        SendMessageToLLM(message:"你好啊",null);
    }
    public void SendMessageToLLM(string message, UnityAction<string> callback)
    {
        StartCoroutine(PostRequest(message, callback));
    }

    IEnumerator PostRequest(string message, UnityAction<string> callback)
    {
        //创建匿名请求体
        var requestBody = new
        {
            model = "deepseek-chat",
            messages = new[]
            {
                new { role = "user", content = message }
            }
        };
        //使用newtonsoft.json序列化
        string jsonBody = JsonConvert.SerializeObject(requestBody);
        Debug.LogWarning(jsonBody);
        //yield return null;
        //创建unityRequest
        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error:" + request.error);//打印错误日志
            Debug.LogError("Response：" + request.downloadHandler.text);
        }
        else
        {
            string responseJson = request.downloadHandler.text;
            Debug.LogWarning("ResponseFileData: "+ responseJson);
            // 解析 JSON
            var responseData = JsonConvert.DeserializeObject<ResponseData>(responseJson);
            if (responseData != null && responseData.choices.Length > 0)
            {
                string responseMessage = responseData.choices[0].message.content;  // 提取消息文本
                Debug.LogWarning("ResponseMessage: "+ responseMessage);
            }
        }
    }
    // 用来解析 API 响应的 C# 类
    [System.Serializable]
    public class ResponseData
    {
        public Choice[] choices;
    }

    [System.Serializable]
    public class Choice
    {
        public Message message;
    }

    [System.Serializable]
    public class Message
    {
        public string role;
        public string content;
    }
}
