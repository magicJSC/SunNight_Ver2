using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_InventorySlot : UI_Base,IDragable,IDroppable
{
    [HideInInspector]
    public UI_Inventory inven;
    [HideInInspector]
    public UI_Item itemUI;


    GameObject explain;
    Text explainText;
    Text nameText;

    public new void Init()
    {
        explain = inven.explain;
        explainText = Util.FindChild(explain, "ExplainText", true).GetComponent<Text>();
        nameText = Util.FindChild(explain, "NameText", true).GetComponent<Text>();

        itemUI = transform.GetComponentInChildren<UI_Item>();

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
    }

    private void ShowSlotInfo(PointerEventData data)
    {
        if (itemUI.slotInfo.itemInfo == null)
            return;
        explain.SetActive(true);
        Set_Explain();
    }

    public void Set_Explain()
    {
        explainText.text = itemUI.slotInfo.itemInfo.explain;
        nameText.text = itemUI.slotInfo.itemInfo.itemName;
        UIScreenPlacement.Tooltip(explain.GetComponent<RectTransform>(), GetComponent<RectTransform>());
    }
}
