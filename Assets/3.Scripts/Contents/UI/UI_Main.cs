using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class UI_Main : UI_Base
{
    AudioSource audioSource;
    public AudioClip clickSound;


    GameObject start;
    GameObject option;
    GameObject exit;

    Animator anim;

    public override void Init()
    {
        start = Util.FindChild(gameObject,"Start",true);
        option = Util.FindChild(gameObject,"Option",true);
        exit = Util.FindChild(gameObject,"Exit",true);

        anim = GetComponent<Animator>();

        UI_EventHandler evt = start.GetComponent<UI_EventHandler>();
        evt._OnClick += (PointerEventData p)=> { anim.Play("Load"); };

        evt = exit.GetComponent<UI_EventHandler>();
        evt._OnClick += (PointerEventData p)=> { Application.Quit(); };
    }

    private void StartGame()
    {
        SceneManager.LoadScene("GameScene");
    }
}
