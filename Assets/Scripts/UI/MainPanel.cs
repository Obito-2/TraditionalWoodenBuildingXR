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

    [Tooltip("与主菜单按钮顺序一一对应；为空则回退为按钮上 ModelMenuButton 或 GameObject 名作为 Addressables 键")]
    [SerializeField] private ExperienceModelCatalog catalog;

    protected override void Awake()
    {
        base.Awake();
        if (catalog == null)
            catalog = Resources.Load<ExperienceModelCatalog>("ExperienceModelCatalog");
        if (catalog == null)
        {
            var main = FindObjectOfType<Main>();
            if (main != null)
                catalog = main.ModelCatalog;
        }

        _modelButtonList = transform.GetComponentsInChildren<Button>();
        if (catalog != null && catalog.entries != null && catalog.entries.Count > 0)
        {
            int n = Mathf.Min(_modelButtonList.Length, catalog.entries.Count);
            if (_modelButtonList.Length != catalog.entries.Count)
                Debug.LogWarning($"[MainPanel] 按钮数量({_modelButtonList.Length})与目录条目数({catalog.entries.Count})不一致，仅绑定前 {n} 个。");

            for (int i = 0; i < n; i++)
            {
                string key = catalog.entries[i].addressableKey;
                _modelButtonList[i].onClick.AddListener(() => Main.Instance.LoadModel(key));
            }
        }
        else
        {
            foreach (var button in _modelButtonList)
                button.onClick.AddListener(() => onButtonClicked(button));
        }
    }
    private void Start()
    {
        // //todo:自动点击，测试代码，需要删除
        // Button FirstButton;
        // FirstButton = _modelButtonList[0];
        // StartCoroutine(MyTools.DelayClickButton(FirstButton));
        // Debug.LogError("自动测试：点击主面板第一个按钮");
    }
    private void onButtonClicked(Button button)
    {
        var slot = button.GetComponent<ModelMenuButton>();
        string key = slot != null && !string.IsNullOrEmpty(slot.AddressableKey)
            ? slot.AddressableKey
            : button.name;
        Main.Instance.LoadModel(key);
    }
}