using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Time : UI_Base
{
    Text timeText;

    public static Animator anim;

    public override void Init()
    {
        timeText = Util.FindChild(gameObject,"Time",true).GetComponent<Text>();

        TimeController.nightEvent += ShowBattleSign;
        TimeController.timeEvent += SetTimeText;

        anim = GetComponent<Animator>();
    }

    void SetTimeText(float time)
    {
        float hour = time / 60;
        float minute = time % 60;
        timeText.text = string.Format("{0:D2}:{1:D2}",(int)hour,(int)minute);
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