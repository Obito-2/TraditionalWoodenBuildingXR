using System;
using System.Collections;
using System.Collections.Generic;
using CodeArchitect.Manager.Event;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using UI;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class JigsawPiece : MonoBehaviour
{
    private JigsawInteraction jigsawInteraction;
    private PointableUnityEventWrapper handGrabEventWrapper;
    private Grabbable _grabbable;

    public bool isJigsawFixed;
    private Outline _outline;
    private InteractPanel _interactPanel;
    
    //监听是否被抓取
    private void Awake()
    {
        _outline = GetComponent<Outline>();

        jigsawInteraction = this.transform.root.GetComponent<JigsawInteraction>();
        _grabbable = this.GetComponent<Grabbable>();
        handGrabEventWrapper = this.GetComponent<PointableUnityEventWrapper>();
        handGrabEventWrapper.InjectPointable(_grabbable);
        
        handGrabEventWrapper.WhenSelect.AddListener(WhenSelectPiece);
        handGrabEventWrapper.WhenUnselect.AddListener(WhenUnselectPiece);
        
        EventCenter.Instance.AddListener<InteractPanel>(EventName.InteractPanelLoadFinish,GetInteractPanel);
    }
    
    //Todo：判断当前组件是否已经组合，如果已经组合，则不会跟随手势控制
    private void GetInteractPanel(InteractPanel panel)
    {
        _interactPanel = panel;
        if (_interactPanel != null)
        {
            Debug.LogWarning("JigsawPiece find InteractPanel success");
        }
        
    }
    private void WhenSelectPiece(PointerEvent pointerEvent)
    {
        //当前piece被selected时，更改与之对应的gohstModel的材质，进行提示
        jigsawInteraction.HeilightPiece(gameObject);
        _outline.enabled = true;
        //展示模型当前信息
        _interactPanel.ShowModelInfo(this.gameObject);
        
    }
    
    private void WhenUnselectPiece(PointerEvent pointerEvent)
    {
        jigsawInteraction.CancelHeilightPiece(gameObject);
        //TODO：在指定范围unselected，才会进行组合判定
        jigsawInteraction.IsJigsawUnselectedParts(gameObject);
        _outline.enabled = false;

    }
    private void OnDestroy()
    {
        EventCenter.Instance.RemoveListener<InteractPanel>(EventName.InteractPanelLoadFinish,GetInteractPanel);
    }
}
