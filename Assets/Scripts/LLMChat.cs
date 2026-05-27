using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CodeArchitect.Manager.Event;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class LLMChat : MonoBehaviour
{
    private const float RAG_TIMEOUT = 10f;

    /// <summary>绕过 SSL 证书验证（Quest 3/Android 需此设置才能访问外部 HTTPS API）</summary>
    private class BypassCertificate : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData) => true;
    }

    private string apiKey;
    private string apiUrl;
    private string modelName;
    //系统提示词
    [SerializeField]
    public SystemPrompt systemPrompt;

    // LangChain 线程 ID，用于维持会话上下文
    private string threadId;

    private void Awake()
    {
        apiKey = LlmEnv.ApiKey;
        apiUrl = LlmEnv.ApiUrl;
        modelName = LlmEnv.Model;

        // 创建或获取 thread ID（每个应用实例一个）
        threadId = LoadOrCreateThreadId();

        //订阅aichat按钮点击事件，获取query，触发发送请求协程
        EventCenter.Instance.AddListener<String>(EventName.AIChat, SendQueryToLLM);
    }

    /// <summary>
    /// 加载或创建 thread ID，用于 LangChain 会话管理
    /// </summary>
    private string LoadOrCreateThreadId()
    {
        const string threadIdKey = "LangChain_ThreadId";
        string saved = PlayerPrefs.GetString(threadIdKey, "");

        if (!string.IsNullOrEmpty(saved))
        {
            Debug.Log($"[LLMChat] 使用已保存的 Thread ID: {saved}");
            return saved;
        }

        // 生成新的 thread ID
        string newThreadId = System.Guid.NewGuid().ToString();
        PlayerPrefs.SetString(threadIdKey, newThreadId);
        PlayerPrefs.Save();
        Debug.Log($"[LLMChat] 创建新 Thread ID: {newThreadId}");
        return newThreadId;
    }

    private void SendQueryToLLM(String interactPanelModelInfo)
    {
        string prefix = ExperienceSession.GetLlmUserQuestionPrefix();
        string userMessage = prefix + interactPanelModelInfo;

        bool hasRag = !string.IsNullOrEmpty(LlmEnv.RagApiUrl);
        bool hasLlm = !string.IsNullOrEmpty(LlmEnv.ApiUrl) && !string.IsNullOrEmpty(LlmEnv.ApiKey);

        if (hasRag)
        {
            StartCoroutine(TryRagWithFallback(userMessage, hasLlm));
        }
        else if (hasLlm)
        {
            StartCoroutine(PostRequest(userMessage, (response) =>
            {
                EventCenter.Instance.TriggerEvent(EventName.LLMResponse, response);
            }));
        }
        else
        {
            EventCenter.Instance.TriggerEvent(EventName.LLMResponse, "出错了: 未配置任何 LLM 接口");
        }
    }

    /// <summary>
    /// RAG 优先，失败或超时自动兜底商业 LLM。
    /// 各阶段通过 LLMResponse 事件更新 UI 状态提示。
    /// </summary>
    IEnumerator TryRagWithFallback(string userMessage, bool hasLlmFallback)
    {
        // 阶段1：正在请求 RAG
        EventCenter.Instance.TriggerEvent(EventName.LLMResponse, "正在搜索知识库…");
        Debug.Log("[LLMChat] 尝试 RAG 请求...");
        string ragResult = null;
        yield return PostRagRequest(userMessage, (response) =>
        {
            ragResult = response;
        });

        bool ragFailed = string.IsNullOrEmpty(ragResult) || ragResult.StartsWith("出错了:");
        if (!ragFailed)
        {
            Debug.Log("[LLMChat] RAG 请求成功");
            EventCenter.Instance.TriggerEvent(EventName.LLMResponse, ragResult);
            yield break;
        }

        Debug.LogWarning($"[LLMChat] RAG 失败，兜底商业 LLM。RAG 结果: {ragResult ?? "null"}");

        if (hasLlmFallback)
        {
            // 阶段2：RAG 失败，切换云端模型
            EventCenter.Instance.TriggerEvent(EventName.LLMResponse, "RAG 服务失败，切换云端模型回答\n正在思考中…");
            Debug.Log("[LLMChat] 开始兜底商业 LLM 请求...");
            string llmResult = null;
            yield return PostRequest(userMessage, (response) =>
            {
                llmResult = response;
            });
            // LLM 结果直接展示（成功或错误信息都让用户看到）
            EventCenter.Instance.TriggerEvent(EventName.LLMResponse, llmResult);
        }
        else
        {
            EventCenter.Instance.TriggerEvent(EventName.LLMResponse, "出错了: RAG 服务不可用，且未配置商业 LLM 兜底");
        }
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

        string systemContent = systemPrompt.prompt;
        if (ExperienceSession.TryGetLlmSystemPromptOverride(out string overridePrompt))
            systemContent = overridePrompt;

        List<Message> messages = new List<Message>
        {
            new Message { role = "system", content = systemContent },
            new Message { role = "user",content = message}
        };
        //组装请求体
        ChatRequest requestBody = new ChatRequest
        {
            model = modelName,
            messages = messages
        };
        //序列化为json
        string jsonBody = JsonUtility.ToJson(requestBody);
        Debug.Log($"[LLMChat] 发送 OpenAI 请求: {apiUrl}, Model: {modelName}");

        //创建unityRequest，发送请求
        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);
        request.certificateHandler = new BypassCertificate();

        yield return request.SendWebRequest();//暂停协程，等网络返回结果，再恢复执行。

        //判断请求是否成功，处理结果
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            string errorMsg = "出错了: " + request.error;
            Debug.LogError($"[LLMChat] OpenAI 请求失败: {errorMsg}");
            callback?.Invoke(errorMsg);
        }
        else
        {
            string responseJson = request.downloadHandler.text;

            string responseMessage = GetResponseMessages(responseJson);
            if (string.IsNullOrEmpty(responseMessage))
            {
                Debug.LogWarning($"[LLMChat] OpenAI 返回空响应: {responseJson}");
            }
            callback?.Invoke(responseMessage);//如果调用方传了回调函数，就执行它，把response信息当作参数传进去
        }
    }

    /// <summary>
    /// RAG 智能体接口（带超时控制）
    /// </summary>
    IEnumerator PostRagRequest(string query, UnityAction<string> callback)
    {
        if (string.IsNullOrEmpty(LlmEnv.RagApiUrl))
        {
            callback?.Invoke("出错了: 未配置 RAG_API_URL");
            yield break;
        }

        // 包含 thread_id 以维持 LangChain 会话
        var requestBody = new RagRequest
        {
            query = query,
            thread_id = threadId
        };
        string jsonBody = JsonUtility.ToJson(requestBody);
        Debug.Log($"[LLMChat] 发送 RAG 请求: {LlmEnv.RagApiUrl}, Thread ID: {threadId}, Query: {query}");

        UnityWebRequest request = new UnityWebRequest(LlmEnv.RagApiUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.timeout = (int)RAG_TIMEOUT;
        request.certificateHandler = new BypassCertificate();

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            string errorMsg = $"出错了: {request.error}";
            Debug.LogError($"[LLMChat] RAG 请求失败: {errorMsg}\nResponse: {request.downloadHandler.text}");
            callback?.Invoke(errorMsg);
            yield break;
        }

        string sseResponse = request.downloadHandler.text;
        Debug.Log($"[LLMChat] 收到 RAG 响应 ({sseResponse.Length} 字符)");

        // 调试输出：显示前 500 个字符的响应格式
        if (sseResponse.Length > 0)
        {
            string preview = sseResponse.Length > 500 ? sseResponse.Substring(0, 500) + "..." : sseResponse;
            Debug.Log($"[LLMChat] RAG 响应预览:\n{preview}");
        }

        string answer = ProcessRagSSEResponse(sseResponse);
        if (string.IsNullOrEmpty(answer))
        {
            Debug.LogWarning("[LLMChat] RAG 返回空答案，原始响应:\n" + sseResponse);
            callback?.Invoke("出错了: 未能获取答案，请检查后端日志");
        }
        else
        {
            callback?.Invoke(answer);
        }
    }

    /// <summary>
    /// 解析 SSE 格式的 RAG 响应
    /// 处理 JSON 对象可能跨越多行的情况
    /// </summary>
    private string ProcessRagSSEResponse(string response)
    {
        System.Text.StringBuilder messageContent = new System.Text.StringBuilder();
        string[] lines = response.Split(new[] { "\r\n", "\n" }, System.StringSplitOptions.None);
        string currentEvent = "";
        System.Text.StringBuilder pendingJsonBuffer = new System.Text.StringBuilder();
        int braceDepth = 0;  // 追踪 { } 的嵌套深度

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];

            if (string.IsNullOrWhiteSpace(line))
                continue;

            if (line.StartsWith("event: "))
            {
                currentEvent = line.Substring("event: ".Length).Trim();
                pendingJsonBuffer.Clear();
                braceDepth = 0;
            }
            else if (line.StartsWith("data: "))
            {
                string dataSegment = line.Substring("data: ".Length).Trim();
                if (string.IsNullOrEmpty(dataSegment))
                    continue;

                // 累积数据，直到 JSON 对象完整
                if (pendingJsonBuffer.Length > 0 || dataSegment.StartsWith("{"))
                {
                    pendingJsonBuffer.Append(dataSegment);

                    // 计算大括号深度
                    foreach (char c in dataSegment)
                    {
                        if (c == '{') braceDepth++;
                        else if (c == '}') braceDepth--;
                    }

                    // 当大括号配对完成且至少有一个完整对象时，开始解析
                    if (braceDepth == 0 && pendingJsonBuffer.Length > 0)
                    {
                        string dataJson = pendingJsonBuffer.ToString();
                        ProcessSingleRagEvent(currentEvent, dataJson, messageContent);
                        pendingJsonBuffer.Clear();
                    }
                }
                else
                {
                    // 直接处理单行完整 JSON
                    ProcessSingleRagEvent(currentEvent, dataSegment, messageContent);
                }
            }
        }

        return messageContent.ToString();
    }

    /// <summary>
    /// 处理单个 RAG SSE 事件
    /// </summary>
    private void ProcessSingleRagEvent(string eventType, string dataJson, System.Text.StringBuilder messageContent)
    {
        try
        {
            switch (eventType)
            {
                case "message":
                    {
                        RagMsgData msgData = JsonUtility.FromJson<RagMsgData>(dataJson);
                        if (msgData != null && !string.IsNullOrEmpty(msgData.content))
                        {
                            messageContent.Append(msgData.content);
                        }
                    }
                    break;

                case "clarification":
                    {
                        RagClarData clarData = JsonUtility.FromJson<RagClarData>(dataJson);
                        if (clarData != null && !string.IsNullOrEmpty(clarData.question))
                        {
                            Debug.Log($"[LLMChat] 收到澄清请求: {clarData.question}");
                        }
                    }
                    break;

                case "error":
                    {
                        RagErrorData errorData = JsonUtility.FromJson<RagErrorData>(dataJson);
                        if (errorData != null)
                        {
                            string errorMsg = $"出错了: [{errorData.code}] {errorData.msg}";
                            Debug.LogError($"[LLMChat] {errorMsg}");
                        }
                    }
                    break;

                case "citations":
                    {
                        RagCitationsData citationsData = JsonUtility.FromJson<RagCitationsData>(dataJson);
                        if (citationsData?.items != null)
                        {
                            AppendCitationImages(citationsData.items, messageContent);
                            Debug.Log($"[LLMChat] 收到 {citationsData.items.Length} 个引文项目");
                        }
                    }
                    break;

                case "agent_trace":
                    // 忽略代理追踪
                    break;

                case "done":
                    Debug.Log("[LLMChat] 响应完成");
                    break;

                default:
                    if (!string.IsNullOrEmpty(eventType))
                    {
                        Debug.LogWarning($"[LLMChat] 未知事件类型: {eventType}");
                    }
                    break;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[LLMChat] 解析 SSE 事件失败: {e.Message}\nEvent: {eventType}\nData: {dataJson}");
        }
    }

    /// <summary>
    /// 将引文中的图片转为 Markdown 图片语法写入 messageContent，实现图文混排
    /// 已存在于 messageContent 中的图片 URL 跳过，避免重复
    /// </summary>
    private void AppendCitationImages(CitationItem[] items, System.Text.StringBuilder messageContent)
    {
        string existingContent = messageContent.ToString();

        foreach (var item in items)
        {
            if (item.type == "image" && item.metadata != null)
            {
                string imageUrl = !string.IsNullOrEmpty(item.metadata.image_uri)
                    ? item.metadata.image_uri
                    : item.metadata.local_path;

                if (!string.IsNullOrEmpty(imageUrl))
                {
                    // 精确检查 messageContent 中是否已有该 URL 的 Markdown 图片语法
                    string escapedUrl = Regex.Escape(imageUrl);
                    if (Regex.IsMatch(existingContent, @"!\[[^\]]*\]\(" + escapedUrl + @"\)"))
                    {
                        Debug.Log($"[LLMChat] 图片已存在于响应中，跳过: {imageUrl}");
                        continue;
                    }

                    string title = item.metadata.title ?? item.content ?? "图片";
                    messageContent.Append($"\n\n![{title}]({imageUrl})\n\n");
                    Debug.Log($"[LLMChat] 嵌入图片: {title} ({imageUrl})");
                }
            }
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

    // ============ RAG 智能体接口数据类 ============
    [System.Serializable]
    private class RagRequest
    {
        public string query;
        public string thread_id;  // LangChain 会话线程 ID
    }

    [System.Serializable]
    private class RagMsgData
    {
        public string content;
    }

    [System.Serializable]
    private class RagClarData
    {
        public string question;
    }

    [System.Serializable]
    private class RagErrorData
    {
        public int code;
        public string msg;
    }

    // ============ Citations 引文数据类 ============
    [System.Serializable]
    private class RagCitationsData
    {
        public CitationItem[] items;
        public object[] relations;
    }

    [System.Serializable]
    private class CitationItem
    {
        public string id;
        public string type;  // "image" 或 "text"
        public string content;
        public CitationMetadata metadata;
        public float score;
    }

    [System.Serializable]
    private class CitationMetadata
    {
        public string title;
        public string image_uri;
        public string local_path;
        public string alt_text;
        public string[] toc_path;
    }

    private void OnDestroy()
    {
        EventCenter.Instance.RemoveListener<String>(EventName.AIChat, SendQueryToLLM);
    }
}