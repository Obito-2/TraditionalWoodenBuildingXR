using System;
using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;

/// <summary>
/// 挂载在模型根节点上，负责控制模型交互各种规则
/// </summary>
public class InteractableManager : MonoBehaviour
{
    [SerializeField]
    private float MinDistanceBetweenPlayers = 0.5f;
    [SerializeField]
    private GameObject[] partModels;//模型子物体

    private GameObject HolisticRayInteraction;
    private Dictionary<GameObject,GameObject> PieceRayInteractionsDic = new Dictionary<GameObject, GameObject>();
    private Transform playerTransform;
    
    private GameObject colliderVisualizer;
    private BoxColliderGizmo boxColliderGizmo;
    
    
    
    private void Awake()
    {
        playerTransform = Camera.main.transform;
        HolisticRayInteraction = this.transform.Find("Holistic_RayGrabInteraction").gameObject;
        if (HolisticRayInteraction != null)
        {
            Debug.LogWarning($"find rayInteractable named {HolisticRayInteraction.name}");
        }

        boxColliderGizmo = GetComponent<BoxColliderGizmo>();
    }
    private void FixedUpdate()
    {
        // 计算玩家与整体模型的距离
        float currentDistance = (this.transform.position - playerTransform.position).magnitude;
        Debug.LogWarning($"家与整体模型的距离：{currentDistance}");
        if (currentDistance > MinDistanceBetweenPlayers)
        {
            HolisticRayInteraction.SetActive(true); // 启用整体模型的射线交互
            Debug.LogWarning("Enabled HolisticRayInteraction");
            boxColliderGizmo.enabled = true;
        }
        else
        {
            HolisticRayInteraction.SetActive(false); // 禁用整体模型的射线交互
            Debug.LogWarning("Disabled HolisticRayInteraction");
            boxColliderGizmo.enabled = false;
        }
    }
}
