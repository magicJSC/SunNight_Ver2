using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeController : BaseController
{
    public Action<float> timeEvent;
    public static Action morningEvent;
    public static Action nightEvent;
    public static Action battleEvent;

    public AudioClip morningAudio;
    public AudioClip nightAudio;

    public float TimeAmount { get { return _time; } set { _time = value; timeEvent?.Invoke(_time); } }
    float _time = 0;

    public static bool finishBattle;

    public enum TimeType
    {
        Morning,
        Night,
        Battle
    }

    public static TimeType timeType { get { return _timeType; } 
        set
        {
            _timeType = value;

            if (_timeType == TimeType.Morning)
                morningEvent?.Invoke();
            else if (_timeType == TimeType.Night)
            {
                if (!finishBattle)
                {
                    nightEvent?.Invoke();
                    _timeType = TimeType.Battle;
                }
            }
            else if(_timeType == TimeType.Battle)
                battleEvent?.Invoke();
        }
    }
    static TimeType _timeType;

    protected override void Init()
    {
        TimeAmount = 360;
        timeType = TimeType.Night;
        morningEvent += SetMorningBGM;
        nightEvent += SetNightBGM;
        StartCoroutine(StartTime());
    }

    private void OnDisable()
    {
        morningEvent = null;
        nightEvent = null;
        battleEvent = null;
    }

    IEnumerator StartTime()
    {
        while (true)
        {
            TimeAmount += Time.deltaTime;
            if (TimeAmount >= 1080 && timeType == TimeType.Morning)
                timeType = TimeType.Night;
            else if (TimeAmount >= 360 && TimeAmount < 1080 && timeType == TimeType.Night)
                timeType = TimeType.Morning;
            else if (TimeAmount >= 1440 && timeType == TimeType.Battle)
            {
                finishBattle = true;
                timeType = TimeType.Night;
            }
            if (TimeAmount >= 1440)
                TimeAmount = 0;
            yield return null;
        }
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
