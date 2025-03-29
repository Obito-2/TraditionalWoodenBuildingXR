using System;
using System.Collections;
using System.Collections.Generic;
using CodeArchitect.Manager.Event;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class ModelInteraction : MonoBehaviour
{
    [SerializeField]
    private GameObject[] partModels;//模型子物体

    public float moveDistance = 0.2f;
    private GameObject _target;//移动方向
    private Vector3[] _moveDirection;//移动方向
    
    public Transform canvasTransform;

    private void Awake()
    {
        _target = new GameObject("ModelCenter")
        {
            transform =
            {
                position = this.transform.position
            }
        };
        _target.transform.SetParent(this.transform);

        GameObject modelVisuals = this.transform.Find("dougong_test").gameObject;

        if (modelVisuals != null)
        {
            partModels = new GameObject[modelVisuals.transform.childCount];
            _moveDirection = new Vector3[modelVisuals.transform.childCount];
        }
        else { Debug.LogWarning("获取整体visualModel失败"); }
        //遍历子物体得到每个物体的 移动方向 与 初始位置
        for (int i = 0; i < modelVisuals.transform.childCount; i++)
        {
            partModels[i] = modelVisuals.transform.GetChild(i).gameObject;
            if (partModels[i] != null)
            {
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
        ShowUI();
    }
    private void ShowUI()
    {
        UI3DManager.Instance.ShowPanelOnSpecificCanvas<InteractPanel>(nameof(InteractPanel), canvasTransform);
    }
    public void ExplodeModel()
    {
        for (int i = 0; i < partModels.Length; i++)
        {
            ModelSplit(partModels[i], -_moveDirection[i]);
        }
        Debug.Log("模型炸开");
    }
    public void CombinationModel()
    {

        for (int i = 0; i < partModels.Length; i++)
        {
            ModelSplit(partModels[i], _moveDirection[i]);
        }

        Debug.Log("模型组合");
    }
    
    private void ModelSplit(GameObject partModel, Vector3 moveDir)
    {
        iTween.MoveAdd(partModel, iTween.Hash("amount", moveDir * moveDistance,
                                                "time", 0.3f,
                                                "space", Space.World,
                                                "easetype", iTween.EaseType.easeInOutQuad,
                                                "looptype", iTween.LoopType.none
            ));
    }
}
