using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeController : MonoBehaviour
{
    public Action warningEvent;
    public  Action<float> timeEvent;
    public  Action<int> dayEvent;
    public  Action morningEvent;
    public  Action nightEvent;

    public AudioClip morningAudio;
    public AudioClip nightAudio;

    public float TimeAmount 
    { 
        get { return _time; }
        set
        { 
            _time = value; 
            timeEvent?.Invoke(_time);
        }
    }
    float _time = 0;
    public float timeSpeed;
     

    public int day = 1;

    public enum TimeType
    {
        Morning,
        Night,
    }

    public TimeType timeType { get { return _timeType; }
        set
        {
            _timeType = value;

            if (_timeType == TimeType.Morning)
            {
                morningEvent?.Invoke();
            }
            else if (_timeType == TimeType.Night)
            {
                nightEvent?.Invoke();
                Managers.Game.canMoveTower = false;
            }
        }
    }
    TimeType _timeType = TimeType.Night;

    public void OnEnable()
    {
        if (TimeAmount >= 1290)
        {
            timeType = TimeType.Night;
        }
        else if (TimeAmount >= 330 && TimeAmount < 1230)
        {
            timeType = TimeType.Morning;
        }
        TimeAmount = 330;
        StartCoroutine(UpdateTime());
    }

    public void SetAction()
    {
        morningEvent += SetMorningBGM;
        nightEvent += SetNightBGM;
    }
    

    IEnumerator UpdateTime()
    {
        while (true)
        {
            if(timeSpeed != 0)
                TimeAmount += Time.deltaTime * timeSpeed;
            if (TimeAmount >= 1290 && timeType == TimeType.Morning)
            {
                timeType = TimeType.Night;
            }
            else if (TimeAmount >= 330 && TimeAmount < 1290 && timeType == TimeType.Night)
            {
                timeType = TimeType.Morning;
            }

            if (TimeAmount >= 1440)
            {
                day++;
                dayEvent?.Invoke(day);
            }
            if(TimeAmount >= 1200 && timeType == TimeType.Morning)
            {
                warningEvent?.Invoke();
            }
            yield return null;
        }
    }

    void SetNightBGM()
    {
        //Managers.Sound.Play(Define.Sound.Bgm,nightAudio);
    }

    void SetMorningBGM()
    {
        //Managers.Sound.Play(Define.Sound.Bgm, morningAudio);
    }
}
