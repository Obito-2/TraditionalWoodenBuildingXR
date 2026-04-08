using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public abstract class PanelManager
{
    protected string canvasTag;

    private GameObject[] canvasArr;

    private List<BasePanel> currentPanels = new List<BasePanel>();

    private Dictionary<string, BasePanel> panelDic = new Dictionary<string, BasePanel>();

    private Dictionary<string, GameObject> canvasDic = new Dictionary<string, GameObject>();

    private Dictionary<string, MethodInfo> ReflectionMethodDic = new Dictionary<string, MethodInfo>();

    public bool IsOperating { get; private set; }


    public int CurrentPanelsCount => currentPanels.Count;
    public bool IsPanelInList
    {
        get
        {
            return currentPanels.Count > 0;
        }
    }

    public PanelManager()
    {
        InitCanvasTag();
        InitCanvas();
    }
    public abstract void InitCanvasTag();
    /// <summary>
    /// 找到场景中所有WorldSpace模式的Canvas，添加到字典中
    /// WorldSpace模式的Canvas需要添加上 Canvas3D 的 Tag
    /// 进入新的场景后要手动调用此方法进行该场景Canvas3D的初始化管理
    /// </summary>
    public virtual void InitCanvas()
    {
        canvasArr = GameObject.FindGameObjectsWithTag(canvasTag);
        if (canvasArr != null)
        {
            for (int i = 0; i < canvasArr.Length; i++)
            {
                if (!canvasDic.ContainsKey(canvasArr[i].name))
                {
                    canvasDic.Add(canvasArr[i].name, canvasArr[i]);
#if UNITY_EDITOR
                    Debug.Log(canvasArr[i].name);
#endif
                }

            }
        }
    }
    public void ClearCanvas()
    {
        canvasArr = null;
        canvasDic?.Clear();
    }
    public virtual GameObject GetCanvasByName(string canvasName)
    {
        for(int i = 0; i < canvasArr.Length; i++)
        {
            if (canvasArr[i].name == canvasName)
            {
                return canvasArr[i];
            }
        }
        return null;
    }
       /// <summary>
       /// 动态添加Canvas到字典中
       /// </summary>
       /// <param name="canvas"></param>
    public virtual void AddCanvasToDic(GameObject canvas)
    {
        if (canvas != null)
        {
            if (!canvasDic.ContainsKey(canvas.name))
            {
                canvasDic.Add(canvas.name, canvas);
            }
            
        }
    }
    public virtual void RemoveCanvasFromDic(string canvasName)
    {
        if (canvasDic.ContainsKey(canvasName))
        {
            canvasDic.Remove(canvasName);
        }
        
    }
    /// <summary>
    /// 显示面板
    /// </summary>
    /// <typeparam name="T">面板脚本类</typeparam>
    /// <param name="panelName">面板名</param>
    /// <param name="onFinish">面板加载完毕后的委托</param>
    public virtual void ShowPanel<T>(string panelName, string canvasName, Action<T> onFinish = null, Action<T> onBegin = null, bool needSavePanel = false, ResourceLoadWay resourceLoadWay = ResourceLoadWay.Addressables) where T : BasePanel
    {

        if (!canvasDic.ContainsKey(canvasName))
        {
            Debug.LogWarning($"传入的canvas:{canvasName}名字错误,panel:{panelName}");
            return;
        }
        IsOperating = true;
        if (panelDic.ContainsKey(panelName))
        {
            if (needSavePanel)
            {
                AddCurrentPanel(panelDic[panelName]);
            }
            panelDic[panelName].Show(() =>
            {
                IsOperating = false;
                onFinish?.Invoke(panelDic[panelName] as T);
            },
              () => onBegin?.Invoke(panelDic[panelName] as T));             
            return;
        }
        string panelPath = null;
        if (resourceLoadWay == ResourceLoadWay.Resources)
        {
            panelPath = ResourceName.UIBasePath + panelName;
        }
        else
        {
            panelPath = panelName;
        }
        //加载Resources文件夹下对应目录的UI预设体，默认路径写在了ResourceName中
        ResourceManager.Instance.LoadAsync<GameObject>(panelPath,
            (obj) =>
            {
                obj.name = panelName;
                //把UI设为对应Canvas的子物体
                obj.transform.SetParent(canvasDic[canvasName].transform, false);
                T panel = obj.GetComponent<T>();
                panelDic.Add(panelName, panel);
                panel.Show(() =>
                {
                    if (needSavePanel)
                    {
                        if (panelDic.ContainsKey(panelName))
                        {
                            AddCurrentPanel(panelDic[panelName]);
                        }                        
                    }
                    IsOperating = false;
                    onFinish?.Invoke(panel);
                }, () =>
                {
                    onBegin?.Invoke(panel);
                });
            });
    }
    public virtual void ShowPanelOnSpecificCanvas<T>(string panelName, Transform canvas, Action<T> onFinish = null, Action<T> onBegin = null, ResourceLoadWay resourceLoadWay = ResourceLoadWay.Addressables) where T : BasePanel
    {
        IsOperating = true;
        T panel = canvas.GetComponentInChildren<T>();
        if (panel != null)
        {
            panel.Show(() =>
            {
                IsOperating = false;
                onFinish?.Invoke(panel);
            },
            () =>
            {
                onBegin?.Invoke(panel);
            });
            return;
        }
        string panelPath = null;
        if (resourceLoadWay == ResourceLoadWay.Resources)
        {
            panelPath = ResourceName.UIBasePath + panelName;
        }
        else
        {
            panelPath = panelName;
        }
        //加载Resources文件夹下对应目录的UI预设体，默认路径写在了ResourceName中
        ResourceManager.Instance.LoadAsync<GameObject>(panelPath,
            (obj) =>
            {
                obj.name = panelName;
                //把UI设为对应Canvas的子物体
                obj.transform.SetParent(canvas, false);
                T panel = obj.GetComponent<T>();
                panel.Show(() =>
                {
                    IsOperating = false;
                    onFinish?.Invoke(panel);
                }, () =>
                {
                    onBegin?.Invoke(panel);
                });
            });
    }
    public void ShowSpecificPanel(BasePanel panel, Action<BasePanel> onFinish = null, Action<BasePanel> onBegin = null)
    {
        IsOperating = true;
        panel.Show(() =>
        {
            IsOperating = false;
            onFinish?.Invoke(panel);
        }, () =>
        {
            onBegin?.Invoke(panel);
        });
    }
    public virtual void ShowLastPanelInList(Action onFinish = null, Action onBegin = null)
    {
        if (currentPanels.Count > 0)
        {
            IsOperating = true;
            var panel = currentPanels[currentPanels.Count - 1];
            panel.Show(()=> 
            {
                IsOperating = false;
                onFinish?.Invoke();
            }, onBegin);
        }
    }

    public virtual void HideLastPanelInList(Action onFinish = null, Action onBegin = null, bool needSavePanel = false)
    {
        if (currentPanels.Count > 0)
        {
            IsOperating = true;
            var panel = currentPanels[currentPanels.Count - 1];

            panel.Hide(() =>
            {
                if (!needSavePanel)
                {
                    RemoveCurrentPanel(panel);
                }
                IsOperating = false;
                onFinish?.Invoke();
            }, onBegin);
        }
    }
    public virtual void DestroyLastPanelInList(Action callback = null, Action onBegin = null, bool needSavePanel = false)
    {
        if (currentPanels.Count > 0)
        {
            IsOperating = true; 
            var panel = currentPanels[currentPanels.Count - 1];
            panel.Hide(() =>
            {
                if (!needSavePanel)
                {
                    RemoveCurrentPanel(panel);
                }
                OnDestroyPanel(panel.name);
                IsOperating = false;
                callback?.Invoke();
            }, onBegin);
        }
    }
    public void HideAllPanelInList(Action onFinish = null, Action onBegin = null, bool needSavePanel = false)
    {
        if (currentPanels.Count > 0)
        {
            IsOperating = true;
            onBegin?.Invoke();
            List<BasePanel> tempList = currentPanels.ToList();
            if (!needSavePanel)
            {
                for(int i = currentPanels.Count - 1; i >= 0; i--)
                {
                    RemoveCurrentPanel(currentPanels[i]);
                }
            }
            for (int i = tempList.Count - 1; i >= 0; i--)
            {
                var panel = tempList[i]; 
                if(i == 0)
                {
                    panel.Hide(() =>
                    {
                        IsOperating = false;
                        onFinish?.Invoke();
                    });
                }
                else
                {
                    panel.Hide();
                }
               
            }
        }
    }
    public void DestroyAllPanelsInlist(Action onFinish = null, Action onBegin = null, bool needSavePanel = false)
    {
        if (currentPanels.Count > 0)
        {
            IsOperating = true;
            onBegin?.Invoke();
            for (int i = currentPanels.Count - 1; i >= 0; i--)
            {
                var panel = currentPanels[i];
                if (i == 0)
                {
                    panel.Hide(() =>
                    {
                        if (!needSavePanel)
                        {
                            RemoveCurrentPanel(panel);
                        }
                        OnDestroyPanel(panel.name);
                        IsOperating = false;
                        onFinish?.Invoke();
                    }, null);
                }
                else
                {
                    panel.Hide(() =>
                    {
                        if (!needSavePanel)
                        {
                            RemoveCurrentPanel(panel);
                        }
                    }, null);
                }

            }
        }
       
    }
    public virtual void RemovePanelFromPanelDic(string panelName)
    {
        if (panelDic.ContainsKey(panelName))
        {
            panelDic.Remove(panelName);
        }
    }
    /// <summary>
    /// 隐藏面板
    /// </summary>
    /// <param name="panelName">面板名</param>
    /// <param name="onFinish">隐藏面板完毕之后的委托</param>
    public virtual void HidePanel(string panelName, Action onFinish = null, Action onBegin = null, bool needSavePanel = false)
    {
        
        if (panelDic.ContainsKey(panelName))
        {
            IsOperating = true;
            if (!needSavePanel)
            {
                RemoveCurrentPanel(panelDic[panelName]);
            }
            panelDic[panelName].Hide(() =>
            {
                IsOperating = false;
                onFinish?.Invoke();
            }, onBegin);
            
        }
       
    }
    public virtual void HideSpecificPanel(BasePanel panel, Action onFinish = null, Action onBegin = null)
    {
        IsOperating = true;
        panel.Hide(() =>
        {
            IsOperating = false;
            onFinish?.Invoke();
        }, onBegin);
    }
    /// <summary>
    /// 销毁面板
    /// </summary>
    /// <param name="panelName">面板名</param>
    /// <param name="onFinish">销毁面板之后的委托</param>
    public virtual void DestroyPanel(string panelName, Action onFinish = null, Action onBegin = null, bool needSavePanel = false)
    {
        
        if (panelDic.ContainsKey(panelName))
        {
            IsOperating = true;
            panelDic[panelName].Hide(() =>
            {
                if (!needSavePanel)
                {
                    RemoveCurrentPanel(panelDic[panelName]);
                }
                OnDestroyPanel(panelName);
                IsOperating = false;
                onFinish?.Invoke();
            }, onBegin);
        }
    }
    public virtual void DestroySpecificPanel(BasePanel panel, Action onFinish = null, Action onBegin = null)
    {
        IsOperating = true;
        panel.Hide(() =>
        {
            GameObject.Destroy(panel.gameObject);
            IsOperating = false;
            onFinish?.Invoke();          
        }, onBegin);
    }
  
    /// <summary>
    /// 销毁面板时的回调
    /// </summary>
    /// <param name="panelName"></param>
    public void OnDestroyPanel(string panelName)
    {
        GameObject.Destroy(panelDic[panelName].gameObject);
        panelDic.Remove(panelName);
    }
    /// <summary>
    /// 得到面板
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="panelName"></param>
    /// <returns></returns>
    public virtual T GetPanel<T>(string panelName) where T : BasePanel
    {
        if (panelDic.ContainsKey(panelName))
        {
            return panelDic[panelName] as T;
        }
        return null;
    }

    public virtual BasePanel GetPanel(string panelName)
    {
        if (panelDic.ContainsKey(panelName))
        {
            return panelDic[panelName];
        }
        return null;
    }

     public virtual string GetLastPanelName()
     {
        return currentPanels[currentPanels.Count - 1].name;
     }
    /// <summary>
    /// 清空字典
    /// </summary>
    public virtual void Clear()
    {
        panelDic?.Clear();
        canvasDic?.Clear();
        ReflectionMethodDic?.Clear();
        canvasArr = null;
        currentPanels.Clear();
    }
    private void AddCurrentPanel(BasePanel panel)
    {
        
        for (int i = 0; i < currentPanels.Count; i++)
        {
            if (currentPanels[i].name == panel.name)
            {
                return;
            }
        }
        currentPanels.Add(panel);
    }
    private void RemoveCurrentPanel(BasePanel panel)
    {
        for (int i = currentPanels.Count - 1; i >= 0; i--)
        {
            if (currentPanels[i].name == panel.name)
            {
                currentPanels.RemoveAt(i);
                break;
            }
        }

    }
    private MethodInfo GetMethodByReflection(string methodName, BasePanel panel)
    {
        if (panel == null) return null;
        Type panelType = Type.GetType(panel.name);
        return GetType().GetMethod(methodName).MakeGenericMethod(new Type[] { panelType });
    }

    public virtual IEnumerator I_ShowPanelByReflection(string canvasName, Action<BasePanel> onFinish = null, Action<BasePanel> onBegin = null,bool needSavePanel = false, ResourceLoadWay resourceLoadWay = ResourceLoadWay.Addressables)
    {
        IsOperating = true;
        for (int i = 0; i < currentPanels.Count; i++)
        {
            BasePanel panel = currentPanels[i];
            string methodName = nameof(ShowPanel);
            string keyName = panel.name + "_" + methodName;
            if (!ReflectionMethodDic.TryGetValue(keyName, out MethodInfo showPanelMethod))
            {
                showPanelMethod = GetMethodByReflection(methodName, panel);
                AddMethodToDic(keyName, showPanelMethod);
            }
            object[] paramArr;
            if(i == currentPanels.Count - 1)
            {
                Action<BasePanel> onFinishCallback = (obj) =>
                {
                    IsOperating = false;
                    onFinish?.Invoke(panel);
                };
                paramArr = new object[] { panel.name, canvasName, onFinishCallback, onBegin, needSavePanel, resourceLoadWay };
               
            }
            else
            {
                paramArr = new object[] { panel.name, canvasName, null, onBegin, needSavePanel, resourceLoadWay };
            }
            showPanelMethod.Invoke(this, paramArr);
            yield return new WaitForSeconds(panel.ToNextPanelWaitTime);
        }
    }
    /// <summary>
    /// 反射形式显示面板
    /// paramArr:string panelName, string canvasName, Action<T> onFinish, Action<T> onBegin, bool needSavePanel, ResourceLoadWay resourceLoadWay
    /// </summary>
    /// <param name="panel"></param>
    /// <param name="paramArr"></param>
    public virtual void ShowPanelByReflection(BasePanel panel, object[] paramArr)
    {
        IsOperating = true;
        string methodName = nameof(ShowPanel);
        string keyName = panel.name + "_" + methodName;
        if (!ReflectionMethodDic.TryGetValue(keyName, out MethodInfo showPanelMethod))
        {
            showPanelMethod = GetMethodByReflection(methodName, panel);
            AddMethodToDic(keyName, showPanelMethod);
        }
        IsOperating = false;
        showPanelMethod.Invoke(this, paramArr);
    }
    private void AddMethodToDic(string keyName, MethodInfo methodInfo)
    {
        if (!ReflectionMethodDic.ContainsKey(keyName))
        {
            ReflectionMethodDic.Add(keyName, methodInfo);
        }
    }
    private void RemoveMethodFromDic(string keyName)
    {
        if (ReflectionMethodDic.ContainsKey(keyName))
        {
            ReflectionMethodDic.Remove(keyName);
        }
    }
}
