using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Item : UI_Base
{
    [HideInInspector]
    public Transform parDragBefore;
    [HideInInspector]
    public Transform dropingSlot;
    [HideInInspector]
    public StorageManager.SlotInfo slotInfo;

    public AudioClip changeSound;

    Image icon;
    Text count;

    RectTransform rect;

    public new void Init()
    {
        if (_init)
            return;

        _init = true;
        count = Util.FindChild(gameObject, "Count", true).GetComponent<Text>();
        icon = Util.FindChild(gameObject, "Icon", true).GetComponent<Image>();

        parDragBefore = transform.parent;
        rect = GetComponent<RectTransform>();
        UI_EventHandler evt = icon.GetComponent<UI_EventHandler>();
        evt._OnBeginDrag += (PointerEventData p) =>
        {
            if (p.pointerDrag.transform.parent.GetComponentInParent<IDragable>() == null)
                return;
            Managers.Game.mouse.CursorType = Define.CursorType.Drag;
            transform.parent = transform.root.GetChild(0);
            icon.raycastTarget = false;
        };
        evt._OnDrag += (PointerEventData p) =>
        {
            if (p.pointerDrag.transform.parent.GetComponentInParent<IDragable>() != null)
                return;
            transform.position = Input.mousePosition;
        };
        evt._OnEndDrag += (PointerEventData p) =>
        {
            transform.parent = parDragBefore;
            rect.anchoredPosition = Vector2.zero;
            icon.raycastTarget = true;
            Drop();
            Managers.Inven.hotBarUI.CheckChoice();
        };
        evt._OnDrop += (PointerEventData p) =>
        {
            GameObject item = p.pointerDrag;

            if (item.transform.parent.GetComponent<UI_Item>() != null)
                item.transform.parent.GetComponent<UI_Item>().dropingSlot = transform;
        };
        if (slotInfo.itemInfo != null)
            SetInfo();
        else
            SetEmptyItem();
    }


    public void SetInfo()
    {
        if (slotInfo.count == 0)
            MakeEmptySlot();
        else
        {
            SetExistItem();
            count.text = $"{slotInfo.count}";
            icon.sprite = slotInfo.itemInfo.itemIcon;
            rect.anchoredPosition = Vector2.zero;
        }
    }

    public void MakeEmptySlot()
    {
        slotInfo.count = 0;
        slotInfo.itemInfo = null;
        SetEmptyItem();
    }

    public void SetEmptyItem()
    {
        icon.gameObject.SetActive(false);
        count.gameObject.SetActive(false);
    }

    public void SetExistItem()
    {
        icon.gameObject.SetActive(true);
        if (slotInfo.count != 1)
            count.gameObject.SetActive(true);
        else
            count.gameObject.SetActive(false);
    }

    void Drop()
    {
        if (dropingSlot == null)
        {
            AbandonItem();
            return;
        }
        UI_Item s2 = dropingSlot.GetComponentInChildren<UI_Item>();

        if (s2 == this)
            ReturnItemToSlot();
        else if (s2.slotInfo.itemInfo == null)
            Managers.Inven.ChangeItem(this, s2);
        else if (s2.slotInfo.itemInfo.idName != slotInfo.itemInfo.idName)
            Managers.Inven.ChangeItem(this, s2);
        else
            Managers.Inven.AddItem(this, s2);

        Managers.Sound.Play(Define.Sound.Effect, changeSound);
        dropingSlot = null;
    }

    void ReturnItemToSlot()
    {
        rect.anchoredPosition = Vector2.zero;
    }

    void AbandonItem()
    {
        if (slotInfo.itemInfo.itemType == Define.ItemType.Building || !StorageManager.canAbandon)
            return;
        GameObject go = Instantiate(Resources.Load<GameObject>("UI/UI_Abandon"));
        go.GetComponent<UI_Abandon>().itemUI = this;
        rect.anchoredPosition = Vector2.zero;
    }
}
