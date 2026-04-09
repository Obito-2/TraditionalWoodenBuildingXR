using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// ����WorldSpaceģʽ��3D�ռ��У���UI������
/// </summary>
public class UI3DManager : PanelManager
{
    private static UI3DManager instance;
    public static UI3DManager Instance
    {
        get
        {
            if(instance == null)
            {
                instance = new UI3DManager();
            }
            return instance;
        }
    }

    public override void InitCanvasTag()
    {
        canvasTag = "Canvas3D";
    }
    public UI3DManager() : base()
    {

    }
}
