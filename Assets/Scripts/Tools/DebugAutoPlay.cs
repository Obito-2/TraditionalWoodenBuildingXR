using System.Collections;
using CodeArchitect.Manager.Event;
using UI;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 调试脚本：串联自动化测试流程
/// 1. 等待主面板加载
/// 2. 点击主面板第一个按钮
/// 3. 等待模型加载完成
/// 4. 查找并选中"下昂"子物体
/// 5. 点击AI问答按钮
///
/// 使用方法：
/// - 挂载到任意 GameObject 上
/// - 确保 Debug.autoDebugPlay 为 true 时脚本会自动执行
/// 或在 Inspector 中勾选 startAutomatically
/// </summary>
public class DebugAutoPlay : MonoBehaviour
{
    [SerializeField] private bool startAutomatically = true;
    [SerializeField] private float panelCheckInterval = 0.5f; // 检查主面板的间隔
    [SerializeField] private float modelCheckInterval = 0.5f; // 检查模型加载的间隔
    [SerializeField] private string targetPieceName = "下昂"; // 目标子物体名称

    private bool _isRunning = false;
    private MainPanel _mainPanel;
    private InteractPanel _interactPanel;
    private GameObject _loadedModel;
    private System.Action<string> _modelLoadedCallback;

    private void Start()
    {
        if (startAutomatically)
        {
            StartDebugSequence();
        }
    }

    public void StartDebugSequence()
    {
        if (_isRunning)
        {
            Debug.LogWarning("[DebugAutoPlay] 调试序列已在运行中");
            return;
        }

        Debug.Log("[DebugAutoPlay] ========== 开始调试流程 ==========");
        StartCoroutine(AutoPlaySequence());
    }

    private IEnumerator AutoPlaySequence()
    {
        _isRunning = true;

        // 步骤 1：等待主面板加载
        yield return StartCoroutine(WaitForMainPanelLoad());

        // 步骤 2：点击主面板第一个按钮
        yield return StartCoroutine(ClickFirstButton());

        // 步骤 3：等待模型加载完成
        yield return StartCoroutine(WaitForModelLoad());

        // 步骤 4：查找并选中目标零件
        yield return StartCoroutine(SelectTargetPiece());

        // 步骤 5：点击 AI 问答按钮
        yield return StartCoroutine(ClickAIChatButton());

        Debug.Log("[DebugAutoPlay] ========== 调试流程完成 ==========");
        _isRunning = false;
    }

    /// <summary>
    /// 步骤 1：等待主面板加载
    /// </summary>
    private IEnumerator WaitForMainPanelLoad()
    {
        Debug.Log("[DebugAutoPlay] 步骤 1：等待主面板加载...");
        float elapsedTime = 0f;
        float timeout = 10f;

        while (_mainPanel == null)
        {
            _mainPanel = FindObjectOfType<MainPanel>();
            if (_mainPanel != null)
            {
                Debug.Log("[DebugAutoPlay] ✓ 主面板已加载");
                yield break;
            }

            elapsedTime += panelCheckInterval;
            if (elapsedTime > timeout)
            {
                Debug.LogError("[DebugAutoPlay] ✗ 主面板加载超时");
                yield break;
            }

            yield return new WaitForSeconds(panelCheckInterval);
        }
    }

    /// <summary>
    /// 步骤 2：点击主面板第一个按钮
    /// </summary>
    private IEnumerator ClickFirstButton()
    {
        Debug.Log("[DebugAutoPlay] 步骤 2：点击主面板第一个按钮...");

        if (_mainPanel == null)
        {
            Debug.LogError("[DebugAutoPlay] ✗ 主面板不存在，无法点击按钮");
            yield break;
        }

        Button[] allButtons = _mainPanel.GetComponentsInChildren<Button>();
        if (allButtons.Length == 0)
        {
            Debug.LogError("[DebugAutoPlay] ✗ 主面板中未找到按钮");
            yield break;
        }

        // 查找第一个非 Jigsaw 按钮
        Button firstButton = null;
        foreach (var btn in allButtons)
        {
            if (btn.name != "Jigsaw")
            {
                firstButton = btn;
                break;
            }
        }

        if (firstButton == null)
        {
            firstButton = allButtons[0];
        }

        Debug.Log($"[DebugAutoPlay] 点击按钮：{firstButton.name}");
        firstButton.onClick.Invoke();

        yield return new WaitForSeconds(0.5f);
    }

    /// <summary>
    /// 步骤 3：等待模型加载完成
    /// </summary>
    private IEnumerator WaitForModelLoad()
    {
        Debug.Log("[DebugAutoPlay] 步骤 3：等待模型加载完成...");
        float elapsedTime = 0f;
        float timeout = 15f;

        // 监听模型加载事件
        bool modelLoaded = false;
        _modelLoadedCallback = (modelName) =>
        {
            Debug.Log($"[DebugAutoPlay] ✓ 事件收到 - 模型加载完成：{modelName}");
            // 获取加载的模型根节点
            _loadedModel = GameObject.Find(modelName);

            // 如果找不到，尝试加上 (Clone) 后缀（Addressables 加载的预制体会自动添加）
            if (_loadedModel == null)
            {
                _loadedModel = GameObject.Find(modelName + "(Clone)");
            }

            // 如果还是找不到，从场景层级中查找
            if (_loadedModel == null)
            {
                _loadedModel = FindGameObjectByNameInScene(modelName);
            }

            // 最后尝试模糊查找（如果上面都失败）
            if (_loadedModel == null)
            {
                _loadedModel = FindGameObjectByNameInScene(modelName.Replace("(Clone)", ""));
            }

            modelLoaded = true;
        };

        Debug.Log("[DebugAutoPlay] 注册 ModelLoadFinish 事件监听...");
        EventCenter.Instance.AddListener<string>(EventName.ModelLoadFinish, _modelLoadedCallback);

        while (!modelLoaded && elapsedTime < timeout)
        {
            elapsedTime += modelCheckInterval;
            yield return new WaitForSeconds(modelCheckInterval);
        }

        EventCenter.Instance.RemoveListener<string>(EventName.ModelLoadFinish, _modelLoadedCallback);

        // 如果事件没有被触发，尝试直接从场景中查找模型
        if (!modelLoaded)
        {
            Debug.LogWarning("[DebugAutoPlay] ⚠ 事件超时，尝试直接从场景查找模型...");
            // 查找任何包含 InteractPanel 的模型（说明模型已加载）
            _interactPanel = FindObjectOfType<InteractPanel>();
            if (_interactPanel != null)
            {
                // 找到 InteractPanel，说明模型已加载，获取其根节点
                _loadedModel = _interactPanel.transform.root.gameObject;
                Debug.Log($"[DebugAutoPlay] ✓ 从 InteractPanel 获取到模型：{_loadedModel.name}");
                modelLoaded = true;
            }
            else
            {
                Debug.LogError("[DebugAutoPlay] ✗ 模型加载超时，且无法找到 InteractPanel");
                yield break;
            }
        }

        // 再等待一帧让模型完全初始化
        yield return new WaitForSeconds(0.5f);

        // 查找 InteractPanel（如果上面还没有找到的话）
        if (_interactPanel == null)
        {
            _interactPanel = FindObjectOfType<InteractPanel>();
        }

        if (_loadedModel != null)
        {
            Debug.Log($"[DebugAutoPlay] ✓ 模型加载完成：{_loadedModel.name}");
            Debug.Log($"[DebugAutoPlay] 模型层级结构：");
            LogAllChildrenNames(_loadedModel.transform, 0);
        }
        else
        {
            Debug.LogWarning("[DebugAutoPlay] ⚠ 无法获取加载的模型引用");
            Debug.Log("[DebugAutoPlay] 场景中所有根对象：");
            foreach (var go in FindObjectsOfType<GameObject>())
            {
                if (go.transform.parent == null)
                {
                    Debug.Log($"  - {go.name}");
                }
            }
        }
    }

    /// <summary>
    /// 步骤 4：查找并选中目标零件（下昂）
    /// </summary>
    private IEnumerator SelectTargetPiece()
    {
        Debug.Log($"[DebugAutoPlay] 步骤 4：查找并选中目标零件【{targetPieceName}】...");

        if (_loadedModel == null)
        {
            Debug.LogError("[DebugAutoPlay] ✗ 加载的模型不存在，无法查找子物体");
            yield break;
        }

        // 从加载的模型中递归查找目标子物体
        Transform targetTransform = FindChildByName(_loadedModel.transform, targetPieceName);

        if (targetTransform == null)
        {
            Debug.LogError($"[DebugAutoPlay] ✗ 在模型【{_loadedModel.name}】中未找到名为【{targetPieceName}】的子物体");
            LogAllChildrenNames(_loadedModel.transform);
            yield break;
        }

        GameObject targetGameObject = targetTransform.gameObject;
        Debug.Log($"[DebugAutoPlay] ✓ 找到目标零件：{targetGameObject.name}");

        // 获取 PieceModel 组件
        PieceModel pieceModel = targetGameObject.GetComponent<PieceModel>();
        if (pieceModel == null)
        {
            Debug.LogWarning($"[DebugAutoPlay] ⚠ 目标零件【{targetGameObject.name}】上没有 PieceModel 组件，直接触发事件");
        }

        // 模拟选中零件（通过手动触发事件）
        EventCenter.Instance.TriggerEvent(EventName.PieceSelected, targetGameObject);
        Debug.Log("[DebugAutoPlay] ✓ 已触发 PieceSelected 事件");

        yield return new WaitForSeconds(0.5f);
    }

    /// <summary>
    /// 步骤 5：点击 AI 问答按钮
    /// </summary>
    private IEnumerator ClickAIChatButton()
    {
        Debug.Log("[DebugAutoPlay] 步骤 5：点击 AI 问答按钮...");

        if (_interactPanel == null)
        {
            _interactPanel = FindObjectOfType<InteractPanel>();
        }

        if (_interactPanel == null)
        {
            Debug.LogError("[DebugAutoPlay] ✗ InteractPanel 不存在");
            yield break;
        }

        Button[] buttons = _interactPanel.GetComponentsInChildren<Button>();
        Button aichatButton = null;

        foreach (var btn in buttons)
        {
            if (btn.name == "AIChat")
            {
                aichatButton = btn;
                break;
            }
        }

        if (aichatButton == null)
        {
            Debug.LogError("[DebugAutoPlay] ✗ 未找到 AIChat 按钮");
            yield break;
        }

        // 检查按钮是否可交互
        if (!aichatButton.interactable)
        {
            Debug.LogWarning("[DebugAutoPlay] ⚠ AIChat 按钮不可交互，尝试启用...");
            aichatButton.interactable = true;
        }

        Debug.Log("[DebugAutoPlay] 点击 AIChat 按钮");
        aichatButton.onClick.Invoke();

        yield return new WaitForSeconds(0.5f);
        Debug.Log("[DebugAutoPlay] ✓ AI 问答流程已启动");
    }

    /// <summary>
    /// 递归查找子物体（按名称）
    /// </summary>
    private Transform FindChildByName(Transform parent, string name)
    {
        Debug.Log($"[DebugAutoPlay] 查找：{parent.name} 包含 '{name}'？");

        // 检查当前节点
        if (parent.name.Contains(name))
        {
            Debug.Log($"[DebugAutoPlay] ✓ 找到匹配：{parent.name}");
            return parent;
        }

        // 递归检查所有子节点
        foreach (Transform child in parent)
        {
            Transform result = FindChildByName(child, name);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    /// <summary>
    /// 输出模型下所有子物体的名称（用于调试）
    /// </summary>
    private void LogAllChildrenNames(Transform parent, int depth = 0)
    {
        string indent = new string(' ', depth * 2);
        Debug.Log($"[DebugAutoPlay] {indent}- {parent.name}");

        foreach (Transform child in parent)
        {
            LogAllChildrenNames(child, depth + 1);
        }
    }

    /// <summary>
    /// 在场景中根据名称查找 GameObject（支持模糊匹配）
    /// </summary>
    private GameObject FindGameObjectByNameInScene(string name)
    {
        GameObject[] allGameObjects = FindObjectsOfType<GameObject>();

        // 首先尝试精确匹配
        foreach (var go in allGameObjects)
        {
            if (go.name == name)
            {
                return go;
            }
        }

        // 其次尝试包含匹配（处理 Clone 后缀）
        string baseName = name.Replace("(Clone)", "").Trim();
        foreach (var go in allGameObjects)
        {
            if (go.name.StartsWith(baseName))
            {
                return go;
            }
        }

        return null;
    }
}
