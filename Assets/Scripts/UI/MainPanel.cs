using System;
using System.Collections;
using System.Collections.Generic;
using CodeArchitect.Manager.Event;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MainPanel : BaseFadePanel
{
    public Button douGongButton;
    private void Start()
    {
        if (douGongButton != null) {
            douGongButton.onClick.AddListener(ShowModel);
        }
    }
    private void ShowModel()
    {
        ResourceManager.Instance.LoadAsync<GameObject>("DouGong",(obj) =>
            {
                obj.transform.position = new Vector3(0,0,1);
                EventCenter.Instance.TriggerEvent(EventName.ModelLoadFinish,obj.name);
        });
        Debug.LogWarning("模型显示成功");
        //testGIt
    }
    //test2
}
