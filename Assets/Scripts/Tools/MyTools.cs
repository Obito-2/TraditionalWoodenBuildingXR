using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class MyTools
{
    /// <summary>
    /// 如果存在则清楚clone尾缀
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static string RemoveClone(string name)
    {
        if (name.EndsWith("(Clone)"))
        {
            return name.Replace("(Clone)", "").Trim();
        }
        return name;
    }
    /// <summary>
    /// 协程方法自动点击传入button
    /// </summary>
    /// <param name="button"></param>
    /// <returns></returns>
    public static IEnumerator DelayClickButton(Button button)
    {
        yield return new WaitForSeconds(0.5f);//将代码挂起，等待对应的秒数
        if (button != null)
        {
            ExecuteEvents.Execute(button.gameObject, new PointerEventData(EventSystem.current),
                ExecuteEvents.pointerClickHandler);
        }
    }


}
