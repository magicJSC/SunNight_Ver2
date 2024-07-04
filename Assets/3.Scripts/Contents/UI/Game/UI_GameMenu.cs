using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class UI_GameMenu : UI_Base
{
    GameObject continueButton;
    GameObject main;

    public override void Init()
    {
        continueButton = Util.FindChild(gameObject, "Continue", true);
        main = Util.FindChild(gameObject, "Main", true);

        UI_EventHandler evt = continueButton.GetComponent<UI_EventHandler>();
        evt._OnClick += (PointerEventData p) => { Close(); };

        evt = main.GetComponent<UI_EventHandler>();
        evt._OnClick += (PointerEventData p) => { Time.timeScale = 1; SceneManager.LoadScene("MainScene");  };

        GetComponentInParent<PlayerController>().escEvent += ESCKeyEvent;

        Close();
    }

    private void OnEnable()
    {
        Time.timeScale = 0;
    }

    void Close()
    {
        Time.timeScale = 1;
        gameObject.SetActive(false);
    }

    void ESCKeyEvent()
    {
        if(Time.timeScale != 0)
            Time.timeScale = 0;
        else
            Time.timeScale = 1;
        gameObject.SetActive(!gameObject.activeSelf);
    }
}
