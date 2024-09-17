using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static StorageManager;

public class UI_CharcoalSlot : UI_BaseSlot
{
    public ItemSO coalSO;
    private SlotInfo _slotInfo;

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

        itemUI = transform.GetComponentInChildren<UI_Item>();
        itemUI.slotInfo = Managers.Inven.coalSlotInfo;
        itemUI.Init();
        _init = true;
        FillCharcoal();
    }

    private void OnEnable()
    {
        if (!_init || UI_Smelt.isSmelting)
            return;
        FillCharcoal();
    }

    private void OnDisable()
    {
        if (!_init || UI_Smelt.isSmelting)
            return;

        Managers.Inven.AddItems(coalSO, Managers.Inven.smeltUI.charcoalSlot.itemUI.slotInfo.count);
        Managers.Inven.smeltUI.charcoalSlot.itemUI.slotInfo.count = 0;
        Managers.Inven.smeltUI.charcoalSlot.itemUI.SetInfo();
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
        int x, y;
        if (background.anchoredPosition.x <= -510)
            x = 266;
        else
            x = -254;

        if (background.anchoredPosition.y >= 150)
            y = -123;
        else
            y = 257;

        explain.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
        explainText.text = itemUI.slotInfo.itemInfo.explain;
        nameText.text = itemUI.slotInfo.itemInfo.itemName;
    }

    void FillCharcoal()
    {
        int totalCount = 0;
        for (int i = 0; i < Managers.Inven.inventoryUI.slotList.Length; i++)
        {
            _slotInfo = Managers.Inven.inventoryUI.slotList[i].itemUI.slotInfo;
            if (_slotInfo.itemInfo != null)
            {
                if (_slotInfo.itemInfo == coalSO)
                {
                    totalCount += _slotInfo.count;
                    if (totalCount > coalSO.maxAmount)
                    {
                        Managers.Inven.AddItems(_slotInfo.itemInfo, totalCount - _slotInfo.itemInfo.maxAmount);
                        totalCount = _slotInfo.itemInfo.maxAmount;
                        break;
                    }
                    Managers.Inven.inventoryUI.slotList[i].itemUI.MakeEmptySlot();
                }
            }
            else continue;
        }
        Managers.Inven.smeltUI.charcoalSlot.itemUI.slotInfo = new SlotInfo(totalCount,"Coal");
        Managers.Inven.smeltUI.charcoalSlot.itemUI.SetInfo();
    }
}
