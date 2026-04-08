using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ClearUtil 
{
    /// <summary>
    /// ����һЩ���������һ�����Ƴ�����ʱ����
    /// </summary>
   public static void ClearDataInManagers()
    {
        UI3DManager.Instance.Clear();
        EventCenter.Instance.Clear();
        GameObjectPoolManager.Instance.ClearPool();
        ComponentPoolManager.Instance.ClearPool();
        Scheduler.Instance.Clear();
    }

    public static void ClearDataInManagersWithAB()
    {
        ClearDataInManagers();
        AddressablesManager.Instance.Clear();
    }
}
