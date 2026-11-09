using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_HotbarSlot : UI_BaseSlot,IDroppable,IDragable
{
    [HideInInspector]
    public UI_Item itemUI;
    public bool IsBaseSlot { get; set; }

    public override void Init()
    {
       itemUI = transform.GetComponentInChildren<UI_Item>();

        UI_EventHandler evt = GetComponent<UI_EventHandler>();
        evt._OnDrop += OnDrop;
    }

    void OnDrop(PointerEventData p)
    {
        var item = p.pointerDrag != null ? p.pointerDrag.GetComponentInParent<UI_Item>() : null;
        if (item != null)
            item.dropingSlot = IsBaseSlot ? item.parDragBefore : transform;
    }
}
