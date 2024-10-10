using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeController : MonoBehaviour
{
    public static Action surviveEvent;
    public static Action<float> timeEvent;
    public static Action<int> dayEvent;
    public static Action morningEvent;
    public static Action nightEvent;

    public AudioClip morningAudio;
    public AudioClip nightAudio;

    public static float TimeAmount 
    { 
        get { return _time; }
        set
        { 
            _time = value; 
            timeEvent?.Invoke(_time);
        }
    }
    static float _time = 0;
    public static float timeSpeed;
     

    public static int day = 1;

    public enum TimeType
    {
        Morning,
        Night,
    }

    public static TimeType timeType { get { return _timeType; }
        set
        {
            _timeType = value;

            if (_timeType == TimeType.Morning)
            {
                morningEvent?.Invoke();
                dayEvent.Invoke(day);
            }
            else if (_timeType == TimeType.Night)
            {
                nightEvent?.Invoke();
            }
        }
    }
    static TimeType _timeType = TimeType.Night;

    public void Start()
    {
        if (TimeAmount >= 1230)
        {
            timeType = TimeType.Night;
        }
        else if (TimeAmount >= 360 && TimeAmount < 1230)
        {
            timeType = TimeType.Morning;
        }
        StartCoroutine(StartTime());
    }

    public void SetAction()
    {
        morningEvent += SetMorningBGM;
        nightEvent += SetNightBGM;
    }
    

    IEnumerator StartTime()
    {
        while (true)
        {
            if(timeSpeed != 0)
                TimeAmount += Time.deltaTime * timeSpeed;
            if (TimeAmount >= 1230 && timeType == TimeType.Morning)
            {
                timeType = TimeType.Night;
                if (!Managers.Game.completeTutorial)
                    surviveEvent?.Invoke();
            }
            else if (TimeAmount >= 360 && TimeAmount < 1230 && timeType == TimeType.Night)
            {
                timeType = TimeType.Morning;
            }

            if (TimeAmount >= 1440)
            {
                TimeAmount = 0;
            }
            yield return null;
        }
    }

    public static void SetMorning()
    {
        UI_Time.anim.Play("Sleep");
    }

    void SetNightBGM()
    {
        Managers.Sound.Play(Define.Sound.Bgm,nightAudio);
    }

    void SetMorningBGM()
    {
        Managers.Sound.Play(Define.Sound.Bgm, morningAudio);
    }
}
