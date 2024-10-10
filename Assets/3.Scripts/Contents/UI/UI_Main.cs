using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_Main : UI_Base
{
    public AssetReferenceT<AudioClip> clickSoundAsset;
    public AssetReferenceT<AudioClip> enterSoundAsset;

    public AssetReferenceGameObject settingUIAsset;

    GameObject settingUI;

    AudioClip clickSound;
    AudioClip enterSound;


    GameObject start;
    GameObject option;
    GameObject exit;
    Text startText;
    Text optionText;
    Text exitText;

    Animator anim;

    public override void Init()
    {
        anim = GetComponent<Animator>();
        start = Util.FindChild(gameObject,"Start",true);
        option = Util.FindChild(gameObject,"Option",true);
        exit = Util.FindChild(gameObject,"Exit",true);
        startText = start.GetComponentInChildren<Text>();
        optionText = option.GetComponentInChildren<Text>();
        exitText = exit.GetComponentInChildren<Text>();

        UI_EventHandler evt = start.GetComponent<UI_EventHandler>();
        evt._OnClick += (PointerEventData p)=> { StartCoroutine(PlayGame()); Managers.Sound.Play(Define.Sound.Effect, clickSound); };
        evt._OnEnter += (PointerEventData p) => { startText.color = Color.red; Managers.Sound.Play(Define.Sound.Effect, enterSound); };
        evt._OnExit += (PointerEventData p) => { startText.color = Color.black; };

        evt = option.GetComponent<UI_EventHandler>();
        evt._OnEnter += (PointerEventData p) => { optionText.color = Color.red; Managers.Sound.Play(Define.Sound.Effect, enterSound); };
        evt._OnClick += (PointerEventData p) => { Managers.Sound.Play(Define.Sound.Effect, clickSound); settingUI.SetActive(true); };
        evt._OnExit += (PointerEventData p) => { optionText.color = Color.black; };

        evt = exit.GetComponent<UI_EventHandler>();
        evt._OnClick += (PointerEventData p) => { Application.Quit(); };
        evt._OnEnter += (PointerEventData p) => { exitText.color = Color.red; Managers.Sound.Play(Define.Sound.Effect, enterSound); };
        evt._OnExit += (PointerEventData p) => { exitText.color = Color.black; };

        clickSoundAsset.LoadAssetAsync().Completed += (clip) =>
        {
            clickSound = clip.Result;
        };
        enterSoundAsset.LoadAssetAsync().Completed += (clip) =>
        {
            enterSound = clip.Result;
        };

        settingUIAsset.InstantiateAsync().Completed += (obj) =>
        {
            settingUI = obj.Result;
        };
    }


    IEnumerator PlayGame()
    {
        Managers.Game.changeSceneEffecter.StartChangeScene();
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene("TutorialScene");
    }
}
