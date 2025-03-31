using System;
using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class JigsawPiece : MonoBehaviour
{
    private JigsawInteraction jigsawInteraction;
    private PointableUnityEventWrapper handGrabEventWrapper;
    private Grabbable _grabbable;
    
    //监听是否被抓取
    private void Awake()
    {

        jigsawInteraction = this.transform.root.GetComponent<JigsawInteraction>();
        _grabbable = this.GetComponent<Grabbable>();
        handGrabEventWrapper = this.GetComponent<PointableUnityEventWrapper>();
        handGrabEventWrapper.InjectPointable(_grabbable);
        handGrabEventWrapper.WhenSelect.AddListener(WhenSelectPiece);
        handGrabEventWrapper.WhenUnselect.AddListener(WhenUnselectPiece);
    }

    private void WhenSelectPiece(PointerEvent pointerEvent)
    {
        //当前piece被selected时，更改与之对应的gohstModel的材质，进行提示
        jigsawInteraction.HeilightPiece(gameObject);
    }

    private void WhenUnselectPiece(PointerEvent pointerEvent)
    {
        jigsawInteraction.CancelHeilightPiece(gameObject);
        jigsawInteraction.IsJigsawUnselectedParts(gameObject);
    }
}
