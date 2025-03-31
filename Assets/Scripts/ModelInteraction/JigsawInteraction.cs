using System;
using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// 该脚本挂载在模型父物体上，用于提供拼图相关的方法
/// </summary>
public class JigsawInteraction : MonoBehaviour
{
    private MaterialChoice materialChoice;
    public float scatterRadius = 1f; // 拼图块分散的半径

    [SerializeField] private GameObject[] _partModels;
    private GameObject[] _partGhostModels;

    private Dictionary<GameObject, partModelsTransformData> initialDataDic =
        new Dictionary<GameObject, partModelsTransformData>();
    public Dictionary<GameObject,GameObject> partsMappedGhostPartsDic = new Dictionary<GameObject,GameObject>();


    //提供组合整体模型与虚影模型展示逻辑、组合判定、自动吸附组合动画、错误组合提示
    private void Awake()
    {
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
        JigsawScatter(_partModels);
        CreatGhostVisuale(this.gameObject.name);
    }

    private void JigsawScatter(GameObject[] partModels)
    {
        foreach (GameObject part in partModels)
        {
            //将当前模型构件分散至球形平面上
            Vector3 randomDirection = Random.onUnitSphere;
            Vector3 scatterPosition = transform.position + randomDirection * scatterRadius;
            part.transform.position = scatterPosition;
            part.transform.rotation = Random.rotation;

            //记录分散后的构件的position与rotation
            initialDataDic[part] = new partModelsTransformData(part.transform.position, part.transform.rotation);
        }
    }
    private void CreatGhostVisuale(String modelName)
    {
        modelName = MyTools.RemoveClone(this.gameObject.name) + "_Ghost";
        //在特定位置加载ghost模型
        ResourceManager.Instance.LoadAsync<GameObject>(modelName, (obj) =>
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
                if (_partGhostModels != null)
                {
                    partsMappedGhostPartsDic[_partModels[i]] = _partGhostModels[i];
                }
            }
            Debug.LogWarning("获取part对应ghostPiece字典成功");
        });
    }
    //selected构件模型时，展示构件虚影
    public void HeilightPiece(GameObject partModel)
    {
        GameObject mappedGhostPiece = partsMappedGhostPartsDic[partModel];
        MeshRenderer meshRenderer = mappedGhostPiece.GetComponent<MeshRenderer>();
        if (meshRenderer != null && mappedGhostPiece != null)
        {
            meshRenderer.material = materialChoice.materialElements[1].material;
            Debug.LogWarning("修改材质成功");
        }
        else
        {
            Debug.LogError("设置颜色失败，meshrender 或映射gohst = null");
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
        else
        {
            Debug.LogError("设置颜色失败，meshrender 或映射gohst = null");
        }
        
    }
    //unselected构件时，执行逻辑判定
    //自动吸附组合
    //错误组合
    //模型全部组合完成

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
}