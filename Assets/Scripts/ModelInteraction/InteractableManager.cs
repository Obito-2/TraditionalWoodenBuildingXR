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
    private float MinDistanceBetweenPlayers = 2.0f;
    [SerializeField]
    private GameObject[] partModels;//模型子物体

    private GameObject HolisticRayInteraction;
    private Dictionary<GameObject,GameObject> PieceRayInteractionsDic = new Dictionary<GameObject, GameObject>();
    private Transform playerTransform;
    
    private GameObject colliderVisualizer;
    
    
    
    private void Awake()
    {
        playerTransform = Camera.main.transform;
        HolisticRayInteraction = this.transform.Find("Holistic_RayGrabInteraction").gameObject;
        if (HolisticRayInteraction != null)
        {
            Debug.LogWarning($"find rayInteractable named {HolisticRayInteraction.name}");
        }
    }
    
    void Start()
    {
        GameObject modelVisuals = transform.Find("dougong_test")?.gameObject;
        if (modelVisuals != null)
        {
            partModels = new GameObject[modelVisuals.transform.childCount];
        }
        for (int i = 0; i < partModels.Length; i++)
        {
            partModels[i] = modelVisuals.transform.GetChild(i).gameObject;
            
            GameObject rayInteractionObject = partModels[i].transform.Find("ISDK_RayGrabInteraction")?.gameObject;
            PieceRayInteractionsDic[partModels[i]] = rayInteractionObject;
        }
    }
    //todo:bug 不能正常开启和关闭，且不能这次显示box
    void Update()
    {
        // 计算玩家与整体模型的距离
        float currentDistance = (this.transform.position - playerTransform.position).magnitude;
        if (currentDistance > MinDistanceBetweenPlayers)
        {
            if (!HolisticRayInteraction.activeSelf)
            {
                HolisticRayInteraction.SetActive(true); // 启用整体模型的射线交互
                VisualizeCollider(true); // 显示碰撞器
                Debug.LogWarning("Enabled HolisticRayInteraction");
            }
        }
        else
        {
            if (HolisticRayInteraction.activeSelf)
            {
                HolisticRayInteraction.SetActive(false); // 禁用整体模型的射线交互
                Debug.LogWarning("Disabled HolisticRayInteraction");
                VisualizeCollider(false); // 隐藏碰撞器
            }
        }
    }
    private void VisualizeCollider(bool isShow)
    {
        if (isShow)
        {
            // 如果 colliderVisualizer 还没有创建，则创建一个
            if (colliderVisualizer == null)
            {
                colliderVisualizer = GameObject.CreatePrimitive(PrimitiveType.Cube);
                // 设置物体的材质为绿色透明
                Material material = new Material(Shader.Find("Standard"));
                material.color = new Color(0f, 1f, 0f, 0.3f); 
                colliderVisualizer.GetComponent<Renderer>().material = material;
                colliderVisualizer.transform.SetParent(transform);
            }
            BoxCollider boxCollider = gameObject.GetComponent<BoxCollider>();
            colliderVisualizer.transform.localPosition = boxCollider.center;
            colliderVisualizer.transform.localScale = boxCollider.size;
        }
        else
        {
            if (colliderVisualizer != null)
            {
                Destroy(colliderVisualizer);
                colliderVisualizer = null; 
            }
        }
    }
}
    //Todo：以下功能需要实现
    //模型的手势抓取交互、射线抓取、transorm自由变换交互
        //模型与玩家距离小于临界值，关闭射线交互
        
    //模型构件的手势抓取交互、射线抓取、远距离手势交互、手势识别控制
        //玩家与模型构件距离小于临界值时，关闭射线交互、远距离手势交互
        //拼图过程中，如果模型构件已经组合，则关闭所有交互
        
    //面板的射线交互、手势交互
        //无规则限制，任何时候均开启
