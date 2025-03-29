using System;
using System.Threading;
using UnityEngine;
using CodeArchitect.Manager.Event;
using Oculus.Interaction.HandGrab;
using UI;
using UnityEngine.Rendering;
using UnityEngine.UI;

/// <summary>
/// main使用单例模式，提供方法用于控制面板切换、加载模型、重新生成模型
/// </summary>
public class Main : MonoBehaviour
{
    public HandPoseInteraction handPoseInteraction;
    public GameObject _mainCanvas;
    private static Main _instance;//私有静态变量，属于类本身而不属于某个实例
    public static Main Instance//公共静态属性，封装金泰字段的访问
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<Main>();
            }

            if (_instance == null)
            {
                GameObject obj = new GameObject("Main");
                _instance = obj.AddComponent<Main>();
            }
            return _instance;
        }
    }
    void Start()
    {
        //显示菜单panel
        UI3DManager.Instance.ShowPanel<MainPanel>(nameof(MainPanel), CanvasName.MainCanvas);
    }
    public void LoadModel(Button button)
    {
            ResourceManager.Instance.LoadAsync<GameObject>(button.name,(obj) =>
            {
                obj.transform.position = new Vector3(0,0,1);
                EventCenter.Instance.TriggerEvent(EventName.ModelLoadFinish,obj.name);
                UI3DManager.Instance.HidePanel("MainPanel", () =>
                {
                    _mainCanvas.SetActive(false);
                });
                Debug.LogWarning($"{obj.name}与 交互面板 模型显示成功");
            });
    }
    
    public void RespawnModel(string modelName)
    {
        GameObject abandonedModel = GameObject.Find(modelName);
        if (abandonedModel != null)
        {
            Destroy(abandonedModel);
            modelName = RemoveClone(modelName);
            ResourceManager.Instance.LoadAsync<GameObject>(modelName,
                (obj) =>
                {
                    obj.transform.position = new Vector3(0, 0, 1);
                    EventCenter.Instance.TriggerEvent(EventName.ModelLoadFinish,obj.name);
                });

            Debug.LogWarning("模型重新成功成功");
        }
        else
        {
            Debug.LogError($"没有找到模型{modelName}");
        }
    }
    private string RemoveClone(string name)
    {
        if (name.EndsWith("(Clone)"))
        {
            return name.Replace("(Clone)", "").Trim();
        }
        return name;
    }
    void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

}
