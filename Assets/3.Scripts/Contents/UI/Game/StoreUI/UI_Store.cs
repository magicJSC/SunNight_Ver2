using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Store : UI_Base
{

    GameObject close;
    
    Text timeText;

    private void Start()
    {
        close = Util.FindChild(gameObject, "Close", true);
        close.GetComponent<UI_EventHandler>()._OnClick += Close;

        timeText = Util.FindChild<Text>(gameObject, "Time", true);

        GetComponentInParent<Seller>().timerEvent += SetTimeText;

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
