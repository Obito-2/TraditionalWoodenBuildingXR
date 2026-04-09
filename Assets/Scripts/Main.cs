using System;
using System.Collections;
using System.Threading;
using UnityEngine;
using CodeArchitect.Manager.Event;
using Oculus.Interaction.HandGrab;
using UI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// main使用单例模式，挂载在Main空物体上，用于显示主面板
/// 对外提供方法用于控制面板切换、加载模型、重新生成模型
///点击jigsaw进行切换场景
/// </summary>
public class Main : MonoBehaviour
{
    [SerializeField] private ExperienceModelCatalog modelCatalog;

    /// <summary>供 MainPanel 等与 Inspector 中同一份目录引用。</summary>
    public ExperienceModelCatalog ModelCatalog => modelCatalog;

    //控制拼图相关参数，如自动吸附距离、吸附动画时长、虚影材质
    public GameObject _mainCanvas;
    private static Main _instance;//私有静态变量，属于类本身而不属于某个实例
    public static Main Instance//公共静态属性，封装静态字段的访问
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<Main>();
            if (_instance == null)
            {
                GameObject obj = new GameObject("Main");
                _instance = obj.AddComponent<Main>();
            }
            return _instance;
        }
    }
    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        // Quest XR 标准设置：关闭 vSync（由 XR 运行时接管帧同步），锁定目标帧率
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 72;

        if (modelCatalog == null)
            modelCatalog = Resources.Load<ExperienceModelCatalog>("ExperienceModelCatalog");
    }

    void Start()
    {
        //显示菜单panel
        UI3DManager.Instance.ShowPanel<MainPanel>(nameof(MainPanel), CanvasName.MainCanvas);
    }
    public void LoadModel(String modelName)
    {
        ExperienceModelEntry entry = ResolveEntry(modelName);
        ExperienceSession.BeginExperience(entry, modelName);

        ResourceManager.Instance.LoadAsync<GameObject>(modelName,(obj) =>
            {
                obj.transform.position = entry.spawnPosition;
                EventCenter.Instance.TriggerEvent(EventName.ModelLoadFinish,obj.name);
                UI3DManager.Instance.HidePanel("MainPanel", () =>
                {
                    if (_mainCanvas != null)
                        _mainCanvas.SetActive(false);
                });
                Debug.LogWarning($"{obj.name}与 交互面板 模型显示成功");
            });
    }

    ExperienceModelEntry ResolveEntry(string addressableKey)
    {
        if (modelCatalog != null)
        {
            var e = modelCatalog.GetEntryOrDefault(addressableKey);
            if (e != null)
                return e;
        }

        return ExperienceModelEntry.CreateFallback(addressableKey);
    }
    public void RespawnModel(string modelName)
    {
        GameObject abandonedModel = GameObject.Find(modelName);
        if (abandonedModel != null)
        {
            Destroy(abandonedModel);
            modelName = MyTools.RemoveClone(modelName);
            var entry = ResolveEntry(modelName);
            ExperienceSession.BeginExperience(entry, modelName);
            ResourceManager.Instance.LoadAsync<GameObject>(modelName,
                (obj) =>
                {
                    obj.transform.position = entry.spawnPosition;
                    EventCenter.Instance.TriggerEvent(EventName.ModelLoadFinish,obj.name);
                });

            Debug.LogWarning("模型重新成功成功");
        }
        else
        {
            Debug.LogError($"没有找到模型{modelName}");
        }
    }

    public void LoadSceneAsync(string sceneName, Action onComplete = null)
    {
        StartCoroutine(LoadSceneCoroutine(sceneName, onComplete));
    }

    private IEnumerator LoadSceneCoroutine(string sceneName, Action onComplete)
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        yield return asyncOperation;
        Debug.LogWarning($"Scene loaded: {sceneName}");
        onComplete?.Invoke();
    }
    
    //控制拼图相关参数，如自动吸附距离、吸附动画时长、虚影材质
    void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

}
