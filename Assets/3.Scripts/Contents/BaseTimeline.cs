using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BaseTimeline : MonoBehaviour
{
    [SerializeField]
    AudioClip clip;

    RectTransform skip;

    private void Start()
    {
        skip = Util.FindChild<RectTransform>(gameObject, "Skip", true);
        UI_EventHandler evt = skip.GetComponent<UI_EventHandler>();
        evt._OnClick += (PointerEventData p) => EndTimeline();
        evt._OnEnter += (PointerEventData p) => { skip.sizeDelta = new Vector3(200,200); };
        evt._OnExit += (PointerEventData p) => { skip.sizeDelta = new Vector3(130,130); };
    }

    public void StartTimeline()
    {
        Time.timeScale = 0;
    }

    public void EndTimeline()
    {
        Time.timeScale = 1;
        Managers.Sound.Play(Define.Sound.Bgm,clip);
        Destroy(gameObject);
    }
}
