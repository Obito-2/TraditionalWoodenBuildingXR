using System;
using System.Collections;
using System.Collections.Generic;
using CodeArchitect.Manager.Event;
using Oculus.Interaction;
using UnityEngine;
using Random = UnityEngine.Random;
using DG.Tweening;
using UnityEngine.SceneManagement;

/// <summary>
/// 挂载在模型父物体上，负责实现拼图交互逻辑：
/// - 初始化构件与虚影模型（Ghost）
/// - 提供拼图吸附/判定/动画反馈
/// - 高亮提示虚影区域
/// </summary>

public class JigsawInteraction : MonoBehaviour
{

    public RectTransform canvasTransform;
    public RectTransform backCanvasTransform;
    public float scatterRadius = 1f; // 拼图块分散的半径
    public float JigsawMinDistance = 1f;

    [SerializeField] private GameObject[] _partModels;
    private GameObject[] _partGhostModels;

    public Dictionary<GameObject, GameObject> partsMappedGhostPartsDic = new Dictionary<GameObject, GameObject>();
    private MaterialChoice materialChoice;

    public AudioTrigger JigsawSuccrssAudio;
    public AudioTrigger JigsawFailAudio;
    

    //提供组合整体模型与虚影模型展示逻辑、组合判定、自动吸附组合动画、错误组合提示
    private void Awake()
    {
        materialChoice = GetComponent<MaterialChoice>();
        if (materialChoice == null)
        {
            Debug.LogError("materialChoice获取失败");
        }
        GameObject modelVisuals = this.transform.Find("dougong_test").gameObject;
        modelVisuals.transform.SetParent(backCanvasTransform);
        //获取当前整体模型的所有构件
        _partModels = new GameObject[modelVisuals.transform.childCount];
        for (int i = 0; i < _partModels.Length; i++)
        {
            _partModels[i] = modelVisuals.transform.GetChild(i).gameObject;
        }
    }
    private void Start()
    {
        UI3DManager.Instance.ShowPanelOnSpecificCanvas<JigsawPanel>(nameof(JigsawPanel), canvasTransform);
        CreatGhostVisuale(gameObject.name);
    }
    
    public void CreatGhostVisuale(String modelName)
    {
        String LoadModelName = MyTools.RemoveClone(modelName) + "_Ghost";
        //在特定位置加载ghost模型,并建立映射字典
        ResourceManager.Instance.LoadAsync<GameObject>(LoadModelName, (obj) =>
        {
            obj.transform.position = this.transform.position;
            obj.transform.rotation = this.transform.rotation;
            obj.transform.SetParent(this.transform);
            // //获取ghost模型的所有构件
            GameObject modelVisuals = obj.transform.Find("dougong_test").gameObject;
            _partGhostModels = new GameObject[modelVisuals.transform.childCount];

            for (int i = 0; i < _partGhostModels.Length; i++)
            {
                _partGhostModels[i] = modelVisuals.transform.GetChild(i).gameObject;
                if (_partGhostModels[i] != null)
                {
                    partsMappedGhostPartsDic[_partModels[i]] = _partGhostModels[i];
                }
            }
            Debug.LogWarning("获取part对应ghostPiece字典成功");
        });
    }
    /// <summary>
    /// 提供方法：根据距离判定用户unselected的构件是否可以进行拼图，并展示对应的交互效果
    /// </summary>
    /// <param name="partModel"></param>
    public void IsJigsawUnselectedParts(GameObject partModel)
    {
        GameObject mappedGhostPiece = partsMappedGhostPartsDic[partModel];
        if (mappedGhostPiece != null)
        {
            float distanceMagnitude = (mappedGhostPiece.transform.position - partModel.transform.position).magnitude;
            bool IsCUrrentUnselectedCombinate = distanceMagnitude <= JigsawMinDistance;
            if (IsCUrrentUnselectedCombinate)
            {
                partModel.transform.position = mappedGhostPiece.transform.position;
                partModel.transform.rotation = mappedGhostPiece.transform.rotation;
                //关闭ghost模型渲染
                MeshRenderer meshRenderer = mappedGhostPiece.GetComponent<MeshRenderer>();
                meshRenderer.enabled = false;
                JigsawSuccrssAudio.PlayAudio();
                EventCenter.Instance.TriggerEvent(EventName.PieceJigsawed, partModel);//组合成功触发事件，通知manager关闭当前物体的交互
                partModel.transform.SetParent(transform);
                Debug.LogWarning($"组合piece成功");
            }
            else
            {
                //组合失败，当前构件进行shake,
                partModel.transform.DOShakePosition(0.5f, 1f, 10, 90, false, true)
                    .SetEase(Ease.InOutSine);
                JigsawFailAudio.PlayAudio();
                partModel.transform.SetParent(transform);
                Debug.LogWarning("组合失败");
            }
        }
        else
        {
            Debug.LogError($"没能找到{partModel.name}对应的ghost模型");
        }
    }
    
    //selected构件模型时，修改ghostModel的材质进行提示
    public void HeilightPiece(GameObject partModel)
    {
        GameObject mappedGhostPiece = partsMappedGhostPartsDic[partModel];
        MeshRenderer meshRenderer = mappedGhostPiece.GetComponent<MeshRenderer>();
        if (meshRenderer != null && mappedGhostPiece != null)
        {
            meshRenderer.material = materialChoice.materialElements[2].material;
            Debug.LogWarning("修改材质成功");
        }
    }
    public void CancelHeilightPiece(GameObject partModel)
    {
        GameObject mappedGhostPiece = partsMappedGhostPartsDic[partModel];
        MeshRenderer meshRenderer = mappedGhostPiece.GetComponent<MeshRenderer>();
        if (meshRenderer != null && mappedGhostPiece != null)
        {
            meshRenderer.material = materialChoice.materialElements[0].material;
            Debug.LogWarning("修改材质成功");
        }
    }
}