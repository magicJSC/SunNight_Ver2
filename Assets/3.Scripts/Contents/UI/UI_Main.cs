using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class UI_Main : UI_Base
{
    GameObject start;
    GameObject option;
    GameObject exit;

    public override void Init()
    {
        start = Util.FindChild(gameObject,"Start",true);
        option = Util.FindChild(gameObject,"Option",true);
        exit = Util.FindChild(gameObject,"Exit",true);

        UI_EventHandler evt = start.GetComponent<UI_EventHandler>();
        evt._OnClick += StartGame;

        evt = exit.GetComponent<UI_EventHandler>();
        evt._OnClick += (PointerEventData p)=> { Application.Quit(); };
    }

    private void StartGame(PointerEventData p)
    {
        SceneManager.LoadScene("GameScene");
    }
}
