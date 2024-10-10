using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class UI_GameMenu : UI_Base
{
    GameObject continueButton;
    GameObject main;

    Animator anim;

    public override void Init()
    {
        anim = GetComponent<Animator>();

        continueButton = Util.FindChild(gameObject, "Continue", true);
        main = Util.FindChild(gameObject, "Main", true);

        UI_EventHandler evt = continueButton.GetComponent<UI_EventHandler>();
        evt._OnClick += (PointerEventData p) => { Close(); };

        evt = main.GetComponent<UI_EventHandler>();
        evt._OnClick += (PointerEventData p) => { Time.timeScale = 1; Managers.Data.Save(); StartCoroutine(MainScene()); };

        Close();
    }

    private void OnEnable()
    {
        Time.timeScale = 0;
        Managers.UI.PopUIList.Add(gameObject);
    }

    private void OnDisable()
    {
        Time.timeScale = 1;
        Managers.UI.PopUIList.Remove(gameObject);
    }

    void Close()
    {
        Time.timeScale = 1;
        gameObject.SetActive(false);
    }

    public IEnumerator MainScene()
    {
        Managers.Game.changeSceneEffecter.StartChangeScene();
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene("MainScene");
    }
}
