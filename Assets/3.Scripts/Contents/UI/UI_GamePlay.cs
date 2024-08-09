using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_GamePlay : MonoBehaviour
{
    public AssetReferenceT<AudioClip> clickSoundAsset;
    public AssetReferenceT<AudioClip> enterSoundAsset;

    AudioClip clickSound;
    AudioClip enterSound;


    GameObject story;
    GameObject tutorial;
    RectTransform close;

    Text storyText;
    Text tutorialText;

    void Start()
    {
        story = Util.FindChild(gameObject, "Game", true);
        tutorial = Util.FindChild(gameObject, "Tutorial", true);
        close = Util.FindChild<RectTransform>(gameObject, "Close", true);
        storyText = Util.FindChild<Text>(gameObject, "GameText", true);
        tutorialText = Util.FindChild<Text>(gameObject, "TutorialText", true);

        UI_EventHandler evt = story.GetComponent<UI_EventHandler>();
        evt._OnClick += (PointerEventData p) => { GetComponent<Animator>().Play("StartGame"); Managers.Sound.Play(Define.Sound.Effect, clickSound); };
        evt._OnEnter += (PointerEventData p) => { storyText.color = Color.red; Managers.Sound.Play(Define.Sound.Effect, enterSound); };
        evt._OnExit += (PointerEventData p) => { storyText.color = Color.white; };

        evt = tutorial.GetComponent<UI_EventHandler>();
        evt._OnClick += (PointerEventData p) => { GetComponent<Animator>().Play("Tutorial"); Managers.Sound.Play(Define.Sound.Effect, clickSound); };
        evt._OnEnter += (PointerEventData p) => { tutorialText.color = Color.red; Managers.Sound.Play(Define.Sound.Effect, enterSound); };
        evt._OnExit += (PointerEventData p) => { tutorialText.color = Color.white; };

        evt = close.GetComponent<UI_EventHandler>();
        evt._OnClick += (PointerEventData p) => { gameObject.SetActive(false); };
        evt._OnEnter += (PointerEventData p) => { close.sizeDelta = new Vector2(100, 100); };
        evt._OnExit += (PointerEventData p) => { close.sizeDelta = new Vector2(80, 80); };

        clickSoundAsset.LoadAssetAsync().Completed += (clip) =>
        {
            clickSound = clip.Result;
        };
        enterSoundAsset.LoadAssetAsync().Completed += (clip) =>
        {
            enterSound = clip.Result;
        };

        gameObject.SetActive(false);
    }

    void MainGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    void Tutorial()
    {
        SceneManager.LoadScene("TutorialScene");
    }
}
