using UI;
using UnityEngine;

/// <summary>
/// 该脚本挂载在模型物体身上
/// 当用户调用 ExplodeModel 或 CombinationModel 方法时，
/// 模型的子物体会按指定的方向移动，从而实现模型的"炸开"和"组合"效果。
/// </summary>
public class ModelInteraction : MonoBehaviour
{
    [SerializeField]
    private GameObject[] partModels;//存储模型子物体
    public float moveDistance = 0.2f;
    private GameObject _target;//移动方向
    private Vector3[] _moveDirection;//子物体移动方向

    [Tooltip("包含所有零件的直接父节点名；为空则优先读取 ExperienceSession.ActiveEntry.partsRootName，最终回退为 dougong_test")]
    [SerializeField] private string partsRootName = "dougong_test";

    public Transform canvasTransform;

    private void Awake()
    {
        // 优先使用 ExperienceSession 中的配置，允许运行时按模型覆盖
        var activeEntry = ExperienceSession.ActiveEntry;
        if (activeEntry != null && !string.IsNullOrEmpty(activeEntry.partsRootName))
            partsRootName = activeEntry.partsRootName;

        //创建一个新的空物体，用于作为模型的中心
        _target = new GameObject("ModelCenter")
        {
            transform =
            {
                position = this.transform.position
            }
        };
        _target.transform.SetParent(this.transform);
        //查找零件父节点子物体
        GameObject modelVisuals = this.transform.Find(partsRootName)?.gameObject;
        if (modelVisuals != null)
        {
            //初始化子物体数组和移动方向数组
            partModels = new GameObject[modelVisuals.transform.childCount];
            _moveDirection = new Vector3[modelVisuals.transform.childCount];
        }
        else { Debug.LogWarning($"获取整体visualModel失败，partsRootName={partsRootName}"); return; }
        //遍历子物体通过和目标物体作向量减法，得到每个物体的 移动方向 与 初始位置
        for (int i = 0; i < modelVisuals.transform.childCount; i++)
        {
            partModels[i] = modelVisuals.transform.GetChild(i).gameObject;
            if (partModels[i] != null)
            {
                // 计算每个子物体的移动方向，方向从子物体到目标物体
                _moveDirection[i] = (_target.transform.position - partModels[i].transform.position).normalized;
            }
            else
            {
                Debug.LogWarning($"获取子物体失败 name:{modelVisuals.transform.GetChild(i).gameObject.name}");
            }
        }
    }
    private void Start()
    {
        //在指定的UI画布上显示交互面板
        UI3DManager.Instance.ShowPanelOnSpecificCanvas<InteractPanel>(nameof(InteractPanel), canvasTransform);
    }
    //爆炸模型的方法，触发所有子物体的移动
    public void ExplodeModel()
    {
        for (int i = 0; i < partModels.Length; i++)
        {
            //调用PartModelMove方法，向外移动子物体
            PartModelMove(partModels[i], -_moveDirection[i]);
        }
        Debug.Log("模型炸开");
    }
    // 组合模型的方法，恢复所有子物体到原始位置
    public void CombinationModel()
    {
        for (int i = 0; i < partModels.Length; i++)
        {
            // 调用 PartModelMove 方法，向内移动子物体
            PartModelMove(partModels[i], _moveDirection[i]);
        }
        Debug.Log("模型组合");
    }
    // 通过iTween插件实现子物体的平滑移动
    private void PartModelMove(GameObject partModel, Vector3 moveDir)
    {
        iTween.MoveAdd(partModel, iTween.Hash("amount", moveDir * moveDistance,
                                                "time", 0.3f,
                                                "space", Space.World,
                                                "easetype", iTween.EaseType.easeInOutQuad,
                                                "looptype", iTween.LoopType.none
            ));
    }
}
