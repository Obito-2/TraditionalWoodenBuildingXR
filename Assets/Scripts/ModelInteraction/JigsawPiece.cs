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

    public bool isJigsawFixed;
    
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
    
    //Todo：判断当前组件是否已经组合，如何已经组合，则不会跟随手势控制
    //todo：hover当前构件，构件outline提示，并在panel面板中展示构件信息
}
