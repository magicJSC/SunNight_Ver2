using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_Abandon : UI_Base
{
    [HideInInspector]
    public UI_Item itemUI;

    RectTransform correct;
    RectTransform reject;

    public override void Init()
    {
        correct = Util.FindChild(gameObject,"Correct",true).GetComponent<RectTransform>();
        reject = Util.FindChild(gameObject,"Reject",true).GetComponent<RectTransform>();

        UI_EventHandler evt = correct.GetComponent<UI_EventHandler>();
        evt._OnClick += ClickCorrect;
        evt._OnEnter += EnterCorrect;
        evt._OnExit += ExitCorrect;

        evt = reject.GetComponent<UI_EventHandler>();
        evt._OnClick += ClickReject;
        evt._OnEnter += EnterReject;
        evt._OnExit += ExitReject;

    }

    private void OnEnable()
    {
        Managers.UI.PopUIList.Add(gameObject);
    }

    private void OnDisable()
    {
        Managers.UI.PopUIList.Remove(gameObject);
        Destroy(gameObject);
    }

    void ClickCorrect(PointerEventData p)
    {
        itemUI.MakeEmptySlot();
        gameObject.SetActive(false);
    }

    void EnterCorrect(PointerEventData p)
    {
        correct.sizeDelta = new Vector2(150,150);
    }

    void ExitCorrect(PointerEventData p)
    {
        correct.sizeDelta = new Vector2(100, 100);
    }

    void ClickReject(PointerEventData p)
    {
       gameObject.SetActive(false);
    }

    void EnterReject(PointerEventData p)
    {
        reject.sizeDelta = new Vector2(150, 150);
    }

    void ExitReject(PointerEventData p)
    {
        reject.sizeDelta = new Vector2(100, 100);
    }
}
