
using CodeArchitect.Manager.Event;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 挂载在每个拼图子物体上，处理以下功能：
/// 1. 被抓取时启用高亮效果（Outline）
/// 2. 抓取释放时判定是否拼合成功
/// 3. 与JigsawInteraction 脚本交互，实现组合逻辑
/// </summary>
public class PieceModel : MonoBehaviour
{
    private JigsawInteraction jigsawInteraction;
    private PointableUnityEventWrapper handGrabEventWrapper;
    private Grabbable _grabbable;
    
    public Collider JigsawAreaCollider;
    private Outline _outline;
    
    private DistanceHandGrabInteractable _distanceHandGrabInteractable;
    
    private void Awake()
    {
        Transform root = this.transform.root;
        Collider area = root.GetComponent<BoxCollider>();
        if (area == null) area = root.GetComponent<MeshCollider>();
        if (area == null) area = root.GetComponent<Collider>();
        JigsawAreaCollider = area;
        _outline = GetComponent<Outline>();
        _distanceHandGrabInteractable = this.GetComponent<DistanceHandGrabInteractable>();
        if (SceneManager.GetActiveScene().name == "DouGongJigsaw")
        {
            jigsawInteraction = this.transform.root.GetComponent<JigsawInteraction>();
        }
        _grabbable = this.GetComponent<Grabbable>();
        handGrabEventWrapper = this.GetComponent<PointableUnityEventWrapper>();
        
        handGrabEventWrapper.InjectPointable(_grabbable);
        _distanceHandGrabInteractable.InjectOptionalPointableElement(_grabbable);
        
        handGrabEventWrapper.WhenSelect.AddListener(WhenSelectPiece);
        handGrabEventWrapper.WhenUnselect.AddListener(WhenUnselectPiece);
    }
    /// <summary>
    /// 被抓取时触发：高亮、虚影材质提示、展示当前构件的信息
    /// </summary>
    /// <param name="pointerEvent"></param>
    private void WhenSelectPiece(PointerEvent pointerEvent)
    {
        //当前piece被selected时，更改与之对应的gohstModel的材质，进行提示
        jigsawInteraction?.HeilightPiece(gameObject);
        _outline.enabled = true;
        //触发selected事件，通知订阅方（信息展示面板）展示当前构件的信息
        EventCenter.Instance.TriggerEvent(EventName.PieceSelected,this.gameObject);
        
    }
    /// <summary>
    /// 先判断是否在拼图区域，然后调用jigsawInteraction方法判断是否拼图成功
    /// </summary>
    /// <param name="pointerEvent"></param>
    private void WhenUnselectPiece(PointerEvent pointerEvent)
    {
        jigsawInteraction?.CancelHeilightPiece(gameObject);
        Collider pieceCollider = GetComponent<Collider>();
        
        // 检查当前拼图块的Collider与JigsawAreaCollider是否相交，相交进行判定是否组合
        if (IsColliderIntersecting(pieceCollider, JigsawAreaCollider))
        {
            jigsawInteraction?.IsJigsawUnselectedParts(gameObject);
        }
        _outline.enabled = false;

    }
    private bool IsColliderIntersecting(Collider pieceCollider, Collider areaCollider)
    {
        if (pieceCollider == null || areaCollider == null) return false;
        return pieceCollider.bounds.Intersects(areaCollider.bounds);
    }
}
