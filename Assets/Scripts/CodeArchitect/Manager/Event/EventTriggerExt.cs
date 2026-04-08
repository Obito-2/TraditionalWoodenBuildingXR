using System.Collections;
using System.Collections.Generic;
using CodeArchitect.Manager.Event;
using UnityEngine;

/// <summary>
/// ���ڴ����¼�����չ��
/// </summary>
public static class EventTriggerExt
{
    /// <summary>
    /// �����¼����޲�����
    /// </summary>
    /// <param name="sender">����Դ</param>
    /// <param name="eventName">�¼���</param>
    public static void TriggerEvent(this object sender, EventName eventName)
    {
        EventCenter.Instance.TriggerEvent(eventName);
    }
    /// <summary>
    /// �����¼���1��������
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sender">����Դ</param>
    /// <param name="eventName">�¼���</param>
    /// <param name="info">����</param>
    public static void TriggerEvent<T>(this object sender, EventName eventName, T info)
    {
        EventCenter.Instance.TriggerEvent(eventName, info);
    }
    /// <summary>
    /// �����¼���2��������
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="sender">����Դ</param>
    /// <param name="eventName">�¼���</param>
    /// <param name="info1">����1</param>
    /// <param name="info2">����3</param>
    public static void TriggerEvent<T1, T2>(this object sender, EventName eventName, T1 info1, T2 info2)
    {
        EventCenter.Instance.TriggerEvent(eventName, info1, info2);
    }
    /// <summary>
    /// �����¼���3��������
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <param name="sender">����Դ</param>
    /// <param name="eventName">�¼���</param>
    /// <param name="info1">����1</param>
    /// <param name="info2">����2</param>
    /// <param name="info3">����3</param>
    public static void TriggerEvent<T1, T2, T3>(this object sender, EventName eventName, T1 info1, T2 info2, T3 info3)
    {
        EventCenter.Instance.TriggerEvent(eventName, info1, info2, info3);
    }
    /// <summary>
    /// �����¼���4��������
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <typeparam name="T4"></typeparam>
    /// <param name="sender">����Դ</param>
    /// <param name="eventName">�¼���</param>
    /// <param name="info1">����1</param>
    /// <param name="info2">����2</param>
    /// <param name="info3">����3</param>
    /// <param name="info4">����4</param>
    public static void TriggerEvent<T1, T2, T3, T4>(this object sender, EventName eventName, T1 info1, T2 info2, T3 info3, T4 info4)
    {
        EventCenter.Instance.TriggerEvent(eventName, info1, info2, info3, info4);
    }

    public static R TriggerEventWithReturnValue<R>(this object sender, EventName eventName)
    {
        return EventCenter.Instance.TriggerEventWithReturnValue<R>(eventName);
    }
    public static R TriggerEventWithReturnValue<T1, R>(this object sender, EventName eventName, T1 info1)
    {
        return EventCenter.Instance.TriggerEventWithReturnValue<T1, R>(eventName, info1);
    }
    public static R TriggerEventWithReturnValue<T1,T2, R>(this object sender, EventName eventName, T1 info1, T2 info2)
    {
        return EventCenter.Instance.TriggerEventWithReturnValue<T1,T2, R>(eventName, info1, info2);
    }
    public static R TriggerEventWithReturnValue<T1,T2,T3, R>(this object sender, EventName eventName, T1 info1, T2 info2, T3 info3)
    {
        return EventCenter.Instance.TriggerEventWithReturnValue<T1,T2,T3,R>(eventName, info1, info2, info3);
    }
    public static R TriggerEventWithReturnValue<T1, T2, T3, T4, R>(this object sender, EventName eventName, T1 info1, T2 info2, T3 info3, T4 info4)
    {
        return EventCenter.Instance.TriggerEventWithReturnValue<T1, T2, T3, T4, R>(eventName, info1, info2, info3, info4);
    }
}
