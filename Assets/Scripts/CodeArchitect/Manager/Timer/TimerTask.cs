using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerTask 
{
    /// <summary>
    /// ��ʱʱ��
    /// </summary>
    public float delayedTime;
    /// <summary>
    /// ��ʱ������ʱ�ﵽ��ʱʱ���ִ���¼�
    /// </summary>
    public float timer;
    /// <summary>
    /// �ظ�������-1Ϊһֱ�ظ�
    /// </summary>
    public int repeatTimes;
    /// <summary>
    /// ʣ���ظ�����
    /// </summary>
    public int restRepeatTimes;
    /// <summary>
    /// �¼���ǰ״̬������ͣ�����Ǽ���
    /// </summary>
    public bool isPause;
    /// <summary>
    /// �¼�
    /// </summary>
    public Action callback;

    public TimerTask(float delayedTime, int repeatTimes, Action callback)
    {
        this.delayedTime = delayedTime;
        timer = 0;
        this.repeatTimes = repeatTimes;
        this.isPause = false;
        this.callback = callback;
        if(repeatTimes == -1)
        {
            restRepeatTimes = -1;
        }
        else
        {
            restRepeatTimes = repeatTimes;
        }
    }

}
