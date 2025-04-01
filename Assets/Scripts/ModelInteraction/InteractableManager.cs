using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 挂载在模型根节点上，负责控制模型交互各种规则
/// </summary>
public class InteractableManager : MonoBehaviour
{
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    //Todo：以下功能需要实现
    //模型的手势抓取交互、射线抓取、transorm自由变换交互
        //模型与玩家距离小于临界值，关闭射线交互
        
    //模型构件的手势抓取交互、射线抓取、远距离手势交互、手势识别控制
        //玩家与模型构件距离小于临界值时，关闭射线交互、远距离手势交互
        //拼图过程中，如果模型构件已经组合，则关闭所有交互
        
    //面板的射线交互、手势交互
        //无规则限制，任何时候均开启
}
