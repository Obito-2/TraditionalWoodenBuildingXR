using System;
using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;
using Random = UnityEngine.Random;
using DG.Tweening;

/// <summary>
/// 该脚本挂载在模型父物体上，用于提供拼图相关的方法
/// </summary>

public class JigsawInteraction : MonoBehaviour
{
    private struct partModelsTransformData
    {
        public Vector3 Position;
        public Quaternion Rotation;

        //构造函数，初始化类或结构体
        public partModelsTransformData(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }
    }
    public float scatterRadius = 1f; // 拼图块分散的半径
    public float JigsawMinDistance = 1f;

    [SerializeField] private GameObject[] _partModels;
    private GameObject[] _partGhostModels;

    private Dictionary<GameObject, partModelsTransformData> initialDataDic =
        new Dictionary<GameObject, partModelsTransformData>();

    public Dictionary<GameObject, GameObject> partsMappedGhostPartsDic = new Dictionary<GameObject, GameObject>();
    private MaterialChoice materialChoice;

    public AudioTrigger JigsawSuccrssAudio;
    public AudioTrigger JigsawFailAudio;
    
    private GameObject JigsawCanvas;
    private float padding = 0.2f;

    //提供组合整体模型与虚影模型展示逻辑、组合判定、自动吸附组合动画、错误组合提示
    private void Awake()
    {
        JigsawCanvas = transform.Find("JigsawCanvas").gameObject;
        materialChoice = GetComponent<MaterialChoice>();
        if (materialChoice == null)
        {
            Debug.LogError("materialChoice获取失败");
        }
        GameObject modelVisuals = this.transform.Find("dougong_test").gameObject;
        //获取当前整体模型的所有构件
        _partModels = new GameObject[modelVisuals.transform.childCount];
        for (int i = 0; i < _partModels.Length; i++)
        {
            _partModels[i] = modelVisuals.transform.GetChild(i).gameObject;
        }
    }

    public void JigsawInitial()
    {
        JigsawScatter(JigsawCanvas,_partModels);
        CreatGhostVisuale(this.gameObject.name);
    }
    //TODO：模型分散展示方法，背包系统，将模型picec缩放至统一大小展示于面板上
    private void JigsawScatter(GameObject scatterCanvas,GameObject[] partModels)
    {
        //切换场景

    }
    private void CreatGhostVisuale(String modelName)
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
    public void IsJigsawUnselectedParts(GameObject partModel)
    {
        GameObject mappedGhostPiece = partsMappedGhostPartsDic[partModel];
        if (mappedGhostPiece != null)
        {
            float distanceMagnitude = (mappedGhostPiece.transform.position - partModel.transform.position).magnitude;
            bool IsCUrrentUnselectedCombinate = distanceMagnitude <= JigsawMinDistance;
            if (IsCUrrentUnselectedCombinate)
            {
                //Todo：使用Dotween缓动会存在对不齐问题
                partModel.transform.position = mappedGhostPiece.transform.position;
                partModel.transform.rotation = mappedGhostPiece.transform.rotation;
                //关闭ghost模型渲染
                MeshRenderer meshRenderer = mappedGhostPiece.GetComponent<MeshRenderer>();
                meshRenderer.enabled = false;
                JigsawSuccrssAudio.PlayAudio();
                Debug.LogWarning($"组合成功,当前模型位置");
            }
            else
            {
                //组合失败，当前构件进行shake,
                partModel.transform.DOShakePosition(0.5f, 1f, 10, 90, false, true)
                    .SetEase(Ease.InOutSine);
                JigsawFailAudio.PlayAudio();
                Debug.LogWarning("组合失败");
            }
        }
        else
        {
            Debug.LogError($"没能找到{partModel.name}对应的ghost模型");
        }
    }

    //selected构件模型时，展示构件虚影
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