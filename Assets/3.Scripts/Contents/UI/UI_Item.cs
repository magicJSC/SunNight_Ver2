using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Item : UI_Base
{
    public Transform parDragBefore;
    public Transform dropingSlot;
    public StorageManager.SlotInfo slotInfo;


    Image icon;
    Text count;

    public override void Init()
    {
        if (_init)
            return;

        _init = true;
        count = Util.FindChild(gameObject,"Count",true).GetComponent<Text>();
        icon = Util.FindChild(gameObject, "Icon", true).GetComponent<Image>();

        parDragBefore = transform.parent;

        UI_EventHandler evt = icon.GetComponent<UI_EventHandler>();
        evt._OnBeginDrag += (PointerEventData p) => 
        {
            Managers.Game.mouse.CursorType = Define.CursorType.Drag;
            transform.parent = transform.root.GetChild(0);
            icon.raycastTarget = false;
        };
        evt._OnDrag += (PointerEventData p) => { transform.position = Input.mousePosition;   };
        evt._OnEndDrag += (PointerEventData p) => 
        {
            transform.parent = parDragBefore;
            GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            icon.raycastTarget = true;
            Drop();
            Managers.Inven.CheckHotBarChoice();
        };
        evt._OnDrop += (PointerEventData p) =>
        {
            GameObject item = p.pointerDrag;

            if (item.transform.parent.GetComponent<UI_Item>() != null)
                item.transform.parent.GetComponent<UI_Item>().dropingSlot = transform;
        };
        if(slotInfo.itemInfo != null)
            SetInfo();
        else
            SetEmptyItem();
    }


    public void SetInfo()
    {
        if (slotInfo.keyType == Define.KeyType.Empty)
        { 
            SetEmptyItem();
            return;
        }
        else
            SetExistItem();
        count.text = slotInfo.count.ToString();
        icon.sprite = slotInfo.itemInfo.itemIcon;
        GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        gameObject.SetActive(false);
        gameObject.SetActive(true);
    }

    public void MakeEmptySlot()
    {
        slotInfo.keyType = Define.KeyType.Empty;
        slotInfo.count = 0;
        slotInfo.itemInfo = null;
    }

    public void SetEmptyItem()
    {
        icon.gameObject.SetActive(false);
        count.gameObject.SetActive(false);
    }

    public void SetExistItem()
    {
        icon.gameObject.SetActive(true);
        if (slotInfo.itemInfo.itemType != Define.ItemType.Tool)
            count.gameObject.SetActive(true);
        else
            count.gameObject.SetActive(false);
    }

    void Drop()
    {
        if (dropingSlot == null)
            return;

        UI_Item s2 = dropingSlot.GetComponentInChildren<UI_Item>();

        if(s2.slotInfo.itemInfo.idName != slotInfo.itemInfo.idName)
            Managers.Inven.ChangeItem(this, s2);
        else
            Managers.Inven.AddItem(this, s2);
    }
}
