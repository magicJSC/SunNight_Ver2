using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_HotbarSlot : UI_BaseSlot
{

    public int keyId;
    public UI_Item UI_item;

    public override void Init()
    {

       UI_item = transform.GetComponentInChildren<UI_Item>();

        UI_EventHandler evt = GetComponent<UI_EventHandler>();
        evt._OnDrop += OnDrop;
    }

    void OnDrop(PointerEventData p)
    {
        GameObject item = p.pointerDrag;
        item.transform.parent.GetComponent<UI_Item>().dropingSlot = transform;
    }
}
