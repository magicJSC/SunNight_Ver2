using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_CheckTransport : MonoBehaviour
{
    RectTransform correct;
    RectTransform cancel;

    [HideInInspector]
    public Vector2 spotPos;

    private void Start()
    {
        correct = Util.FindChild<RectTransform>(gameObject,"Correct",true);
        cancel = Util.FindChild<RectTransform>(gameObject,"Cancel",true);

        UI_EventHandler evt = correct.GetComponent<UI_EventHandler>();
        evt._OnClick += (PointerEventData p) => { Managers.Game.tower.transform.position = spotPos; gameObject.SetActive(false); };
        evt._OnEnter += (PointerEventData p) => { correct.sizeDelta = new Vector2(200,200); };
        evt._OnExit += (PointerEventData p) => { correct.sizeDelta = new Vector2(150,150); };

        evt = cancel.GetComponent<UI_EventHandler>();
        evt._OnClick += (PointerEventData p) => { gameObject.SetActive(false); };
        evt._OnEnter += (PointerEventData p) => { cancel.sizeDelta = new Vector2(200, 200); };
        evt._OnExit += (PointerEventData p) => { cancel.sizeDelta = new Vector2(150, 150); };

        gameObject.SetActive(false);
    }
}
