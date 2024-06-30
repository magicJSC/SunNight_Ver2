using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_HotbarSlot : UI_BaseSlot,IDroppable,IDragable
{

    public int keyId;
    public UI_Item UI_item;

    public override void Init()
    {
       UI_item = transform.GetComponentInChildren<UI_Item>();

        UI_EventHandler evt = GetComponent<UI_EventHandler>();
        evt._OnDrop += OnDrop;
        evt._OnEnter += (PointerEventData p) => { Managers.Game.isHandleUI = true; };
        evt._OnExit += (PointerEventData p) => { Managers.Game.isHandleUI = false; };
    }

    void OnDrop(PointerEventData p)
    {
        GameObject item = p.pointerDrag;

        if (item.transform.GetComponentInParent<UI_Item>() != null)
            item.transform.GetComponentInParent<UI_Item>().dropingSlot = transform;
    }
}
