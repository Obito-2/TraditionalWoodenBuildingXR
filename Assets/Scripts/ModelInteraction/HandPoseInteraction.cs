using System;
using System.Collections;
using System.Collections.Generic;
using CodeArchitect.Manager.Event;
using Oculus.Interaction;
using Oculus.Interaction.PoseDetection;
using UnityEngine;

public class HandPoseInteraction : MonoBehaviour
{
    private  ModelInteraction modelInteraction;
    public SelectorUnityEventWrapper PoseLeft;
    public SelectorUnityEventWrapper PoseRight;
    private Action ModelInteractionAction;
    private void Awake()
    {
        PoseLeft.WhenSelected.AddListener(OnPoseLeftSelected);
        PoseRight.WhenSelected.AddListener(OnPoseRightSelected);
        
        EventCenter.Instance.AddListener<String>(EventName.ModelLoadFinish,(modelName) =>
        {
            AddModelInteractionAction(modelName);
        });
    }

    private void Update()
    {
        ModelInteractionAction?.Invoke();
    }

    private void AddModelInteractionAction(string modelName)
    {
        ModelInteractionAction = () =>
        {
            modelInteraction = null;//清除历史模型引用，否则会因为destroy后丢失引用报错
            
            if (modelInteraction == null)
            {
                modelInteraction = GameObject.Find(modelName).GetComponent<ModelInteraction>();
            }
            else
            {
                ClearData();//如果已经找到modelInteraction组件，则清空委托放置重复调用
            }
        };
    }

    private void ClearData()
    {
        ModelInteractionAction = null;
    }
    private void OnPoseLeftSelected()
    {
        modelInteraction?.ExplodeModel();
    }
    private void OnPoseRightSelected()
    {
        modelInteraction?.CombinationModel();
    }
    private void OnDestroy()
    {
        EventCenter.Instance.RemoveListener<String>(EventName.ModelLoadFinish,AddModelInteractionAction);
    }
}
