using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Store : UI_Base
{

    GameObject close;
    RectTransform background;

    Vector3 startPos;

    Text timeText;

    private void Start()
    {
        close = Util.FindChild(gameObject, "Close", true);
        close.GetComponent<UI_EventHandler>()._OnClick += Close;

        timeText = Util.FindChild<Text>(gameObject, "Time", true);

        GetComponentInParent<Seller>().timerEvent += SetTimeText;

        background = Util.FindChild<RectTransform>(gameObject, "Background",true);
        UI_EventHandler evt = background.GetComponent<UI_EventHandler>();
        evt._OnDrag += (PointerEventData p) =>
        {
            background.transform.position = startPos + Input.mousePosition;
            float x = Mathf.Clamp(background.anchoredPosition.x, -400, 400);
            float y = Mathf.Clamp(background.anchoredPosition.y, -115, 60);
            background.anchoredPosition = new Vector2(x, y);
        };
        evt._OnDown += (PointerEventData p) => { startPos = background.transform.position - Input.mousePosition; };
    }

    private void OnEnable()
    {
        Managers.UI.PopUIList.Add(gameObject);
    }

    private void OnDisable()
    {
        Managers.UI.PopUIList.Remove(gameObject);
    }

    void SetTimeText(int time)
    {
        timeText.text = $"{time / 60}:{time % 60}";
    }

    void Close(PointerEventData p)
    {
        gameObject.SetActive(false);
    }
}
