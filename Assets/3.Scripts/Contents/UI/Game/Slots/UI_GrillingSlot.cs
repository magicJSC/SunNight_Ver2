using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using static StorageManager;

public class UI_GrillingSlot : UI_BaseSlot,IDragable,IDroppable
{
    [HideInInspector]
    public UI_Smelt smelt;
    [HideInInspector]
    public UI_Item itemUI;

    GameObject explain;
    Text explainText;
    Text nameText;

    RectTransform background;

    public override void Init()
    {
        explain = smelt.explain;
        explainText = Util.FindChild<Text>(explain, "ExplainText", true);
        nameText = Util.FindChild<Text>(explain, "NameText", true);

        background = GetComponentInParent<RectTransform>();
         
        UI_EventHandler evt = GetComponent<UI_EventHandler>();
        evt._OnEnter += ShowSlotInfo;


        evt._OnExit += (PointerEventData p) =>
        {
            explain.SetActive(false);
        };
        evt._OnDrop += (PointerEventData p) =>
        {
            GameObject item = p.pointerDrag;
            if (item.transform.GetComponentInParent<UI_Item>() != null)
                item.transform.parent.GetComponent<UI_Item>().dropingSlot = transform;
        };

        itemUI = transform.GetComponentInChildren<UI_Item>();
        itemUI.slotInfo = Managers.Inven.grillingSlotInfo;
        itemUI.Init();
        itemUI.SetInfo();
        _init = true;
    }

    private void OnDisable()
    {
        if (!_init || UI_Smelt.isSmelting)
            return;

        if (itemUI.slotInfo.itemInfo != null)
        {
            Managers.Inven.AddItems(itemUI.slotInfo.itemInfo, itemUI.slotInfo.count);
            itemUI.MakeEmptySlot();
        }
        itemUI.SetInfo();
    }

    private void ShowSlotInfo(PointerEventData data)
    {
        if (itemUI.slotInfo.keyType == Define.KeyType.Empty)
            return;
        explain.SetActive(true);
        SetExplain();
    }

    public void SetExplain()
    {
        int x,y;
        if (background.anchoredPosition.x <= -341)
            x = 100;
        else
            x = -417;

        if (background.anchoredPosition.y >= 150)
            y = -123;
        else
            y = 257;

        explain.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
        explainText.text = itemUI.slotInfo.itemInfo.explain;
        nameText.text = itemUI.slotInfo.itemInfo.itemName;

       
    }
}
