using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scheduler : SingletonAutoMonoBase<Scheduler>
{
    private SingleLinkedList<TimerTask> timers =new SingleLinkedList<TimerTask>();
    private TimerTask tempTimer;
    private void Update()
    {
        UpdateTimers();
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
        Clear();
    }
    private void UpdateTimers()
    {
        if (timers.Count == 0) return;
        for(int i=0; i < timers.Count; i++)
        {
            tempTimer = timers[i];
            if (!tempTimer.isPause)
            {
                tempTimer.timer += Time.deltaTime; 
            }
            //�ﵽ�ӳ�ʱ�䣬�����¼�
            if (tempTimer.timer >= tempTimer.delayedTime)
            {
                //print("��ʱ�¼�����");
                tempTimer.callback?.Invoke();
                if (tempTimer.restRepeatTimes == 0)
                {
                    timers.DeleteAt(i);
                    --i;
                    if (timers.Count == 0) break;
                }
                else
                {
                    if (tempTimer.repeatTimes > -1)
                    {
                        tempTimer.restRepeatTimes--;
                    }
                    tempTimer.timer = 0;
                }
            }
            
        }
        
    }
    public void AddTimerTask(float delayedTime, Action callback, int repeatTimes = -1)
    {
        if(callback != null)
        {
            bool isContain = false;
            for (int i = 0; i < timers.Count; i++)
            {
                if (timers[i].callback.Equals(callback))
                {
                    isContain = true;
                    break;
                }
            }
            if (!isContain)
            {
                timers.AddLast(new TimerTask(delayedTime, repeatTimes, callback));
            }
        }
        
    }
    /// <summary>
    /// ��ͣ��ʱ�¼��ļ�ʱ
    /// </summary>
    /// <param name="callback"></param>
    public void Pause(Action callback)
    {
        if (callback != null)
        {
            for (int i = 0; i < timers.Count; i++)
            {
                var taskTemp = timers[i];
                if (taskTemp.callback.Equals(callback))
                {
                    taskTemp.isPause = true;
                }
            }
        }

    }

    /// <summary>
    /// �����¼��ļ�ʱ��ͣ״̬
    /// </summary>
    /// <param name="callback"></param>
    public void UnPause(Action callback)
    {
        if (callback != null)
        {
            for (int i = 0; i < timers.Count; i++)
            {
                var taskTemp = timers[i];
                if (taskTemp.callback.Equals(callback))
                {
                    taskTemp.isPause = false;
                }
            }
        }
    }

    /// <summary>
    /// �Ƴ�ָ���¼�
    /// </summary>
    /// <param name="callback"></param>
    public void Remove(Action callback)
    {
        if (callback != null)
        {
            for (int i = 0; i < timers.Count; i++)
            {
                if (timers[i].callback.Equals(callback))
                {
                    timers.DeleteAt(i);
                }
            }
        }
    }
    /// <summary>
    /// ��ն�ʱ����
    /// </summary>
    public void Clear()
    {
        timers.Clear();
    }


}
