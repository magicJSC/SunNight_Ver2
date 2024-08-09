using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Time : UI_Base
{
    RectTransform timeRullet;
    Text timeText;

    public static Animator anim;

    public override void Init()
    {
        timeRullet = Util.FindChild(gameObject,"TimeRullet",true).GetComponent<RectTransform>();
        timeText = Util.FindChild(gameObject,"Time",true).GetComponent<Text>();

        TimeController.nightEvent += ShowBattleSign;
        TimeController.timeEvent += RotateTimeRullet;
        TimeController.timeEvent += SetTimeText;

        anim = GetComponent<Animator>();
    }

   

    void RotateTimeRullet(float time)
    {
        float angle = 360 / (1440 / time);
        timeRullet.rotation = Quaternion.Euler(0, 0, angle+180);
    }

    void SetTimeText(float time)
    {
        float hour = time / 60;
        float minute = time % 60;
        timeText.text = string.Format("{0:D2}:{1:D2}",(int)hour,(int)minute / 10 * 10);
    }

    void ShowBattleSign()
    { 
        anim.Play("BattleSign");
    }

    void Sleep()
    {
        TimeController.TimeAmount = 360;
    }
}