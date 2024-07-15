using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeController : BaseController
{
    public Action<float> timeEvent;
    public static Action morningEvent;
    public static Action nightEvent;

    public float TimeAmount { get { return _time; } set { _time = value; timeEvent?.Invoke(_time); } }
    float _time = 0;

    public enum TimeType
    {
        Morning,
        Night
    }

    public static TimeType timeType { get { return _timeType; } 
        set
        {
            _timeType = value;

            if(_timeType == TimeType.Morning)
                morningEvent?.Invoke();
            else
                nightEvent?.Invoke();
        }
    }
    static TimeType _timeType;

    protected override void Init()
    {
        TimeAmount = 360;
        morningEvent = null;
        nightEvent = null;
        timeType = TimeType.Night;
    }

    private void Update()
    {
        TimeAmount += Time.deltaTime;
        if(TimeAmount >= 1080 && timeType == TimeType.Morning)
            timeType = TimeType.Night;
        else if(TimeAmount >= 360 && TimeAmount < 1080 && timeType == TimeType.Night)
            timeType = TimeType.Morning;
        if (TimeAmount >= 1440)
            TimeAmount = 0;
    }
}
