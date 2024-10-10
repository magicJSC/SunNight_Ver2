using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Time : MonoBehaviour
{
    Text timeText;
    Text dayText;

    public static Animator anim;

    public void Init()
    {
        timeText = Util.FindChild<Text>(gameObject,"Time",true);
        dayText = Util.FindChild<Text>(gameObject,"Day",true);

        GetComponent<Canvas>().worldCamera = Camera.main;

       

        anim = GetComponent<Animator>();
    }

    public void SetAction()
    {
        TimeController.nightEvent += ShowBattleSign;
        TimeController.timeEvent += SetTimeText;
        TimeController.dayEvent += SetDayText;
        Init();
    }

    void SetDayText(int day)
    {
        dayText.text = $"Day : {day}";
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