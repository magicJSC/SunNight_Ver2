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
    const float Dusk = 21 * 60 + 30;
    const float PlacementRetryInterval = .25f;
    public bool IsNightPlacementPending { get; private set; }
    float nextPlacementAttempt;
     

    public int day = 1;

    public enum TimeType
    {
        Morning,
        Night,
    }

    public TimeType timeType
    {
        get { return _timeType; }
        set
        {
            if (value == TimeType.Night)
            {
                if (_timeType != TimeType.Night) TryBeginNight();
                return;
            }
            IsNightPlacementPending = false;
            _timeType = value;
            morningEvent?.Invoke();
        }
    }
    TimeType _timeType = TimeType.Night;

    public void OnEnable()
    {
        if (TimeAmount >= 330 && TimeAmount < Dusk)
        {
            timeType = TimeType.Morning;
        }
        if (TimeAmount == 0 && day <= 1) TimeAmount = 330;
        StartCoroutine(UpdateTime());
    }

    // Loading a cleared night must not fire nightEvent and spawn the same wave again.
    public void RestoreClock(int savedDay, float savedTime)
    {
        day = savedDay;
        IsNightPlacementPending = false;
        _timeType = savedTime >= 330 && savedTime < Dusk ? TimeType.Morning : TimeType.Night;
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
            if (IsNightPlacementPending)
            {
                if (Time.time >= nextPlacementAttempt) TryBeginNight();
                yield return null;
                continue;
            }
            // The first defense owns the night-to-morning transition. Its enemies, not midnight,
            // decide completion. Player hunger still runs on its own active-game coroutine.
            bool firstDefenseInProgress = FieldSession.Active && day == 1 &&
                timeType == TimeType.Night && TimeAmount >= Dusk;
            if (firstDefenseInProgress)
            { yield return null; continue; }
            if(timeSpeed != 0)
                TimeAmount += Time.deltaTime * timeSpeed;
            if (TimeAmount >= Dusk && timeType == TimeType.Morning)
            {
                TimeAmount = Dusk;
                TryBeginNight();
            }
            else if (TimeAmount >= 330 && TimeAmount < Dusk && timeType == TimeType.Night)
            {
                timeType = TimeType.Morning;
            }

            if (TimeAmount >= 1440)
            {
                TimeAmount -= 1440;
                day++;
                dayEvent?.Invoke(day);
            }
            if(TimeAmount >= 1200 && TimeAmount < Dusk && timeType == TimeType.Morning)
            {
                warningEvent?.Invoke();
            }
            yield return null;
        }
    }

    void TryBeginNight()
    {
        Managers.Game.canMoveTower = false;
        if (TowerMoveController.TryDeployForNight())
        {
            IsNightPlacementPending = false;
            _timeType = TimeType.Night;
            nightEvent?.Invoke();
            return;
        }

        if (!IsNightPlacementPending)
            FieldHUD.Notify("거점을 펼칠 공간이 없습니다. 탁 트인 지면으로 이동하세요.");
        IsNightPlacementPending = true;
        nextPlacementAttempt = Time.time + PlacementRetryInterval;
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
