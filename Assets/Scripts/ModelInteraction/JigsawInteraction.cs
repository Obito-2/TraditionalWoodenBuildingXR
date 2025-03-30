using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class JigsawInteraction : MonoBehaviour
{
    public float scatterRadius = 0.5f; // 拼图块分散的半径
    
    private GameObject _overallModel;
    [SerializeField]
    private GameObject[] _partModels;
    
    private Material _ghostMaterial;
    private Material _transparentMaterial;
    private Dictionary<GameObject,partModelsTransformData> initialDataDic = new Dictionary<GameObject, partModelsTransformData>();
    
    //提供组合整体模型与虚影模型展示逻辑、组合判定、自动吸附组合动画、错误组合提示
    // Start is called before the first frame update

    private void Awake()
    {
        //获取当前整体模型
        _overallModel = this.gameObject;
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
    }

    public void JigsawScatter(GameObject[] partModels)
    {
        foreach (GameObject part in partModels)
        {
            //将当前模型构件分散至球形平面上
            Vector3 randomDirection = Random.onUnitSphere;
            Vector3 scatterPosition = _overallModel.transform.position + randomDirection * scatterRadius;
            part.transform.position = scatterPosition;
            part.transform.rotation = Random.rotation;
            
            //记录分散后的构件的position与rotation
            initialDataDic[part] = new partModelsTransformData(part.transform.position, part.transform.rotation);
        }
        
    }

    //selected模型时，展示构件虚影
    //unselected构件时，执行逻辑判定
    //自动吸附组合
    //错误组合
    //模型全部组合完成
    // Update is called once per frame
    
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
