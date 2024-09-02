using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeController : BaseController
{
    public static Action tutorialEvent;
    public static Action<float> timeEvent;
    public static Action morningEvent;
    public static Action nightEvent;
    public static Action battleEvent;

    public AudioClip morningAudio;
    public AudioClip nightAudio;

    public static float TimeAmount { get { return _time; } set { _time = value; if (init) timeEvent?.Invoke(_time); else init = true; } }
    static float _time = 0;
    public static float timeSpeed;

    static bool init = false;


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
            else if (_timeType == TimeType.Battle)
            {
                battleEvent?.Invoke();
                nightEvent?.Invoke();
            }
        }
    }
    static TimeType _timeType = TimeType.Morning;

    protected override void Init()
    {
        SetMorningBGM();
        TimeAmount = 360;
        morningEvent += SetMorningBGM;
        nightEvent += SetNightBGM;
        StartCoroutine(StartTime());
    }

    private void OnDisable()
    {
        morningEvent = null;
        nightEvent = null;
        battleEvent = null;
        timeEvent = null;
    }

    IEnumerator StartTime()
    {
        while (true)
        {
            TimeAmount += Time.deltaTime * timeSpeed;
            if (TimeAmount >= 1080 && timeType == TimeType.Morning)
                timeType = TimeType.Battle;
            else if (TimeAmount >= 360 && TimeAmount < 1080 && timeType == TimeType.Night)
            {
                timeType = TimeType.Morning;
            }
            else if (TimeAmount >= 1440 && timeType == TimeType.Battle)
            {
                timeType = TimeType.Night;
                if (!Managers.Game.completeTutorial)
                    tutorialEvent.Invoke();
            }
            if (TimeAmount >= 1440)
                TimeAmount = 0;
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
