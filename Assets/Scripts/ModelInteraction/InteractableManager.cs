using System;
using System.Collections;
using System.Collections.Generic;
using CodeArchitect.Manager.Event;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using UnityEngine;

/// <summary>
/// 挂载在模型根节点上，
/// 负责控制模型整体交互规则，子物体的交互规则，防止不同手势、射线、远距离交互互相打架
/// </summary>
public class InteractableManager : MonoBehaviour
{
    [SerializeField]
    private float MinDistanceBetweenPlayers = 0.5f;
    private Transform playerTransform;
    
    private GameObject HolisticRayInteraction;
    private GameObject HolisticHandInteraction;
    private GrabFreeTransformer grabFreeTransformer;
    private OneGrabRotateTransformer oneGrabRotateTransformer;
    
    
    private GameObject colliderVisualizer;
    private BoxColliderGizmo boxColliderGizmo;
    
    private Boolean _isHolistModelAnchor = false;
    
    
    private void Awake()
    {
        playerTransform = Camera.main.transform;
        
        HolisticRayInteraction = this.transform.Find("Holistic_RayGrabInteraction").gameObject;
        HolisticHandInteraction = this.transform.Find("Holistic_HandGrabInteraction").gameObject;
        grabFreeTransformer = this.GetComponent<GrabFreeTransformer>();
        oneGrabRotateTransformer = this.GetComponent<OneGrabRotateTransformer>();
        
        if (HolisticRayInteraction == null || HolisticHandInteraction == null)
        {
            Debug.LogError($"找不到整体模型interactable物体");
        }
        boxColliderGizmo = GetComponent<BoxColliderGizmo>();
        
        EventCenter.Instance.AddListener<GameObject>(EventName.PieceJigsawed,AnchorPieceJigsawed);
    }
    /// <summary>
    /// 固定帧更新，判断玩家与模型的距离，并自动启用/禁用射线交互、碰撞器可视化
    /// </summary>
    private void FixedUpdate()
    {
        if (_isHolistModelAnchor == false)
        {
            // 计算玩家与整体模型的距离
            float currentDistance = (this.transform.position - playerTransform.position).magnitude;
            if (currentDistance > MinDistanceBetweenPlayers )
            {
                HolisticRayInteraction.SetActive(true); // 启用整体模型的射线交互
                Debug.LogWarning("Enabled HolisticRayInteraction");
                boxColliderGizmo.enabled = true;
            }
            if (currentDistance < MinDistanceBetweenPlayers)
            {
                HolisticRayInteraction.SetActive(false); // 禁用整体模型的射线交互
                Debug.LogWarning("Disabled HolisticRayInteraction");
                boxColliderGizmo.enabled = false;
            }
        }
    }
    /// <summary>
    /// 当某个拼图组件完成拼接时，禁用该部分的交互组件（避免重复操作）
    /// </summary>
    /// <param name="pieceJigsawed"></param>
    private void AnchorPieceJigsawed(GameObject pieceJigsawed)
    {
        OffPieceInteractables<HandGrabInteractable>(pieceJigsawed);
        OffPieceInteractables<RayInteractable>(pieceJigsawed);
        OffPieceInteractables<DistanceHandGrabInteractable>(pieceJigsawed);
    }

    /// <summary>
    /// 通用方法：禁用指定类型的交互组件（如果存在）
    /// </summary>
    /// <param name="pieceModel"></param>
    /// <typeparam name="T"></typeparam>
    private void OffPieceInteractables<T>(GameObject pieceModel) where T: MonoBehaviour
    {
        T interactableComponent = pieceModel.GetComponentInChildren<T>();
        if (interactableComponent == null) return;
        // 禁用该组件
        if (interactableComponent.enabled)
        {
            interactableComponent.enabled = false;
        }
        Debug.LogWarning($"{typeof(T).Name} component disabled on {pieceModel.name}");
    }
    /// <summary>
    /// 设置模型是否为“锚定”状态
    /// </summary>
    /// <param name="isAnchor"></param>
    public void IsModelAnchor(bool isAnchor)
    {
        if (isAnchor)
        { 
            //取消当前ghost整体模型的抓取、缩放、射线,开启旋转
            HolisticHandInteraction.SetActive(false);
            HolisticRayInteraction.SetActive(false);
            grabFreeTransformer.enabled = false;
            oneGrabRotateTransformer.enabled = true;
            Debug.LogWarning("模型固定");
            _isHolistModelAnchor = true;
            
        }else
        {
            //开启模型的抓取、射线
            HolisticHandInteraction.SetActive(true);
            HolisticRayInteraction.SetActive(false);
            grabFreeTransformer.enabled = true;
            oneGrabRotateTransformer.enabled = false;
            Debug.LogWarning("模型解锁");
            _isHolistModelAnchor = false;
        }
    }

    private void OnDestroy()
    {
        EventCenter.Instance.RemoveListener<GameObject>(EventName.PieceJigsawed,AnchorPieceJigsawed);
    }
}
