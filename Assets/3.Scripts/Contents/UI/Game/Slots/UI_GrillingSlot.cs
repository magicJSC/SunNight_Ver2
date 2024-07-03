using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UI_GrillingSlot : UI_BaseSlot,IDragable,IDroppable
{
    public UI_Smelt smelt;
    public UI_Item itemUI;

    GameObject explain;
    Text explainText;
    Text nameText;

    public new void Init()
    {
        explain = smelt.explain;
        explainText = Util.FindChild(explain, "ExplainText", true).GetComponent<Text>();
        nameText = Util.FindChild(explain, "NameText", true).GetComponent<Text>();

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
        explain.GetComponent<RectTransform>().anchoredPosition = new Vector2(-417, 257);
        explainText.text = itemUI.slotInfo.itemInfo.explain;
        nameText.text = itemUI.slotInfo.itemInfo.itemName;
    }
}
