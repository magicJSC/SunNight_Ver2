using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class UI_SmeltSlot : UI_BaseSlot,IDragable
{
    [HideInInspector]
    public UI_Smelt smelt;
    [HideInInspector]
    public UI_Item itemUI;

    RectTransform background;

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

        background = GetComponentInParent<RectTransform>();

        evt._OnExit += (PointerEventData p) =>
        {
            explain.SetActive(false);
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
        int x;
        if (background.anchoredPosition.x <= -427)
            x = 187;
        else
            x = -333;

        explain.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, 21);
        explainText.text = itemUI.slotInfo.itemInfo.explain;
        nameText.text = itemUI.slotInfo.itemInfo.itemName;
    }
}
