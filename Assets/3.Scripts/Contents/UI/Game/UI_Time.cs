using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Time : MonoBehaviour
{
    Text timeText;
    Text dayText;
    Text warningText;

    public Animator anim;
    TimeController timeController;

    public void Start()
    {
        timeText = Util.FindChild<Text>(gameObject,"Time",true);
        dayText = Util.FindChild<Text>(gameObject,"Day",true);
        warningText = Util.FindChild<Text>(gameObject,"WarningSign",true);

        GetComponent<Canvas>().worldCamera = Camera.main;

        timeController = Managers.Game.timeController;
        if(timeController.TimeAmount >= 1200 && timeController.TimeAmount < 1290)
        {
            WarningSign();
        }
        else
        {
            warningText.gameObject.SetActive(false);
        }
        anim = GetComponent<Animator>();
    }

    public void SetAction()
    {
        Managers.Game.timeController.nightEvent += ShowBattleSign;
        Managers.Game.timeController.timeEvent += SetTimeText;
        Managers.Game.timeController.dayEvent += SetDayText;
        Managers.Game.timeController.warningEvent += WarningSign;
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

    void WarningSign()
    {
        if (warningText.gameObject.activeSelf)
            return;
        StartCoroutine(CountWarningTime());
    }

    IEnumerator CountWarningTime()
    {
        warningText.gameObject.SetActive(true);
        warningText.text = $"밤까지 남은 시간 :\n{(int)(1290 - timeController.TimeAmount)}분전";
        while (1290 > timeController.TimeAmount)
        {
            yield return null;
            warningText.text = $"밤까지 남은 시간 :\n{(int)(1290 - timeController.TimeAmount)}분전";
        }
        warningText.gameObject.SetActive(false);
    }

    void Sleep()
    {
        Managers.Game.timeController.TimeAmount = 330;
    }

}