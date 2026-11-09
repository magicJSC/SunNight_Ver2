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

    // Loading a cleared night must not fire nightEvent and spawn the same wave again.
    public void RestoreClock(int savedDay, float savedTime)
    {
        day = savedDay;
        _timeType = savedTime >= 330 && savedTime < 1290 ? TimeType.Morning : TimeType.Night;
        TimeAmount = savedTime;
        dayEvent?.Invoke(day);
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
            // The first defense owns the night-to-morning transition. Its enemies, not midnight,
            // decide completion. Player hunger still runs on its own active-game coroutine.
            bool firstDefenseInProgress = FieldSession.Active && day == 1 &&
                timeType == TimeType.Night && TimeAmount >= 1290;
            if (firstDefenseInProgress)
            { yield return null; continue; }
            if(timeSpeed != 0)
                TimeAmount += Time.deltaTime * timeSpeed;
            if (TimeAmount >= 1290 && timeType == TimeType.Morning)
            {
                if (FieldSession.Active && day == 1)
                {
                    TimeAmount = 1290;
                    Debug.Log("FIRST_NIGHT_CLOCK: holding day 1 at 21:30 until defense clear; hunger remains active");
                }
                timeType = TimeType.Night;
            }
            else if (TimeAmount >= 330 && TimeAmount < 1290 && timeType == TimeType.Night)
            {
                timeType = TimeType.Morning;
            }

            if (TimeAmount >= 1440)
            {
                TimeAmount -= 1440;
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
