using System;
using System.Collections;
using System.Collections.Generic;
using CodeArchitect.Manager.Event;
using Oculus.Interaction;
using Oculus.Interaction.PoseDetection;
using UnityEngine;

public class HandPoseInteraction : MonoBehaviour
{
    private ModelInteraction modelInteraction;
    public SelectorUnityEventWrapper PoseLeft;
    public SelectorUnityEventWrapper PoseRight;
    private void Awake()
    {
        modelInteraction = this.transform.root.GetComponent<ModelInteraction>();
        PoseLeft.WhenSelected.AddListener(OnPoseLeftSelected);
        PoseRight.WhenSelected.AddListener(OnPoseRightSelected);
        EventCenter.Instance.AddListener<String>(EventName.ModelLoadFinish,GetModelInteraction);
    }

    //TODO:重新生成模型，不能使用手势交互
    private void GetModelInteraction(String modelName)
    {
        modelInteraction = GameObject.Find(modelName).GetComponent<ModelInteraction>();
    }
    private void OnPoseLeftSelected()
    {
        modelInteraction.ExplodeModel();
    }
    private void OnPoseRightSelected()
    {
        modelInteraction.CombinationModel();
    }
    private void OnDestroy()
    {
        PoseLeft.WhenSelected.RemoveListener(OnPoseLeftSelected);
        PoseRight.WhenSelected.RemoveListener(OnPoseRightSelected);
        EventCenter.Instance.RemoveListener<String>(EventName.ModelLoadFinish,GetModelInteraction);
    }
}
