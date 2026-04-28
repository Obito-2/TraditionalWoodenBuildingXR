using System;
using System.Collections;
using UnityEngine;
using CodeArchitect.Manager.Event;
using Oculus.Interaction.HandGrab;
using UI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 单例，挂载在 Hub 场景的 Main 空物体上：主菜单面板、加载模型、场景切换。
/// 拼装模式入口在 <see cref="MainPanel"/> 的 Jigsaw 按钮；返回 Hub 见 <see cref="LoadHubAndShowMainMenu"/>。
/// </summary>
public class Main : MonoBehaviour
{
    [SerializeField] private ExperienceModelCatalog modelCatalog;

    /// <summary>供 MainPanel 等与 Inspector 中同一份目录引用。</summary>
    public ExperienceModelCatalog ModelCatalog => modelCatalog;

    /// <summary>主菜单 WorldSpace Canvas；Hub 场景重载后需调用 <see cref="RefreshMainCanvasFromLoadedHub"/> 重新绑定。</summary>
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
        }
    }

    public void LoadSceneAsync(string sceneName, Action onComplete = null)
    {
        StartCoroutine(LoadSceneCoroutine(sceneName, onComplete));
    }

    /// <summary>
    /// Hub 场景异步加载完成后调用：重新绑定主菜单 Canvas（DontDestroyOnLoad 上旧引用会随场景卸载失效）。
    /// </summary>
    public void RefreshMainCanvasFromLoadedHub()
    {
        var canvas = GameObject.Find(CanvasName.MainCanvas);
        if (canvas != null)
        {
            _mainCanvas = canvas;
            _mainCanvas.SetActive(true);
        }
    }

    /// <summary>
    /// 从体验/拼装场景返回 Hub 并显示主菜单（刷新 Canvas、InitCanvas、Show MainPanel）。
    /// </summary>
    public void LoadHubAndShowMainMenu(Action onComplete = null)
    {
        LoadSceneAsync(ExperienceSession.HubSceneName, () =>
        {
            RefreshMainCanvasFromLoadedHub();
            UI3DManager.Instance.InitCanvas();
            UI3DManager.Instance.ShowPanel<MainPanel>(nameof(MainPanel), CanvasName.MainCanvas);
            onComplete?.Invoke();
        });
    }

    private IEnumerator LoadSceneCoroutine(string sceneName, Action onComplete)
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        yield return asyncOperation;
        onComplete?.Invoke();
    }

    void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

}
