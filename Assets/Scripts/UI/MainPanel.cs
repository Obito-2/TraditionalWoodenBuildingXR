using System;
using System.Collections;
using System.Collections.Generic;
using CodeArchitect.Manager.Event;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;


public class MainPanel : BaseFadePanel
{
    [SerializeField] private Button[] _modelButtonList;

    protected override void Awake()
    {
        base.Awake();
        _modelButtonList = transform.GetComponentsInChildren<Button>();
        foreach (var button in _modelButtonList)
        {
            button.onClick.AddListener(() => { onButtonClicked(button); }); //使用lambda表达式监听外部作用域函数
        }
    }

    private void Start()
    {
        //todo:自动点击，测试代码，需要删除
        Button FirstButton;
        FirstButton = _modelButtonList[0];
        // StartCoroutine(MyTools.DelayClickButton(FirstButton));
    }
    
    private void onButtonClicked(Button button)
    {
        Main.Instance.LoadModel(button.name);
    }
}