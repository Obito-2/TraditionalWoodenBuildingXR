using System;
using System.Threading;
using UnityEngine;
using CodeArchitect.Manager.Event;
using UI;
using UnityEngine.Rendering;

public class Main : MonoBehaviour
{
    public GameObject _mainCanvas;

    private void Awake()
    {
        EventCenter.Instance.AddListener<String>(EventName.ModelLoadFinish, ChangePanel);
        EventCenter.Instance.AddListener<String>(EventName.RespawnModel, RespawnModel);
    }
    void Start()
    {
        //显示菜单panel
        UI3DManager.Instance.ShowPanel<MainPanel>(nameof(MainPanel), CanvasName.MainCanvas);
    }

    private void ChangePanel(String modelName)
    {
        UI3DManager.Instance.HidePanel("MainPanel", () => { _mainCanvas.SetActive(false); });

    }

    private void RespawnModel(string modelName)
    {
        GameObject AbandonedModel = GameObject.Find(modelName);
        if (AbandonedModel != null)
        {
            Destroy(AbandonedModel);
            modelName = RemoveClone(modelName);
            ResourceManager.Instance.LoadAsync<GameObject>(modelName,
                (obj) =>
                {
                    obj.transform.position = new Vector3(0, 0, 1);
                    EventCenter.Instance.TriggerEvent(EventName.ModelLoadFinish);
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
    private void OnDestroy()
    {
        EventCenter.Instance.RemoveListener<String>(EventName.ModelLoadFinish, ChangePanel);
        EventCenter.Instance.RemoveListener<String>(EventName.RespawnModel, RespawnModel);

    }
    //test
}
