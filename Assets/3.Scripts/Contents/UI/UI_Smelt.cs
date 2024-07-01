using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using static StorageManager;
using System;

/// <summary>
/// 제련 할 때 생성되는 UI입니다
/// </summary>
public class UI_Smelt : UI_Base
{
    UI_GrillingSlot grillingSlot;
    GameObject close;
    UI_SmeltSlot smeltSlot;
    GameObject doSmelt;
    UI_CharcoalSlot charcoalSlot;
    GameObject back;
    public Image timer;
    public GameObject explain;

    public FurnanceController furnanace;
    public bool isSmelting;

    enum GameObjects
    {
        GrillingSlot,
        CloseSmelt,
        SmeltSlot,
        DoSmelt,
        CharcoalSlot,
        ExplainSmelt,
        Timer,
        Background
    }

    public override void Init()
    {
        if (_init)
            return;

        _init = true;
        Bind<GameObject>(typeof(GameObjects));
        grillingSlot = Get<GameObject>((int)GameObjects.GrillingSlot).GetComponent<UI_GrillingSlot>();
        close = Get<GameObject>((int)GameObjects.CloseSmelt);
        back = Get<GameObject>((int)GameObjects.Background);
        smeltSlot = Get<GameObject>((int)GameObjects.SmeltSlot).GetComponent<UI_SmeltSlot>();
        doSmelt = Get<GameObject>((int)GameObjects.DoSmelt);
        charcoalSlot = Get<GameObject>((int)GameObjects.CharcoalSlot).GetComponent<UI_CharcoalSlot>();
        explain = Get<GameObject>((int)GameObjects.ExplainSmelt);
        timer = Get<GameObject>((int)GameObjects.Timer).GetComponent<Image>();

        smeltSlot.smelt = this;
        smeltSlot.Init();

        grillingSlot.smelt = this;
        grillingSlot.Init();

        charcoalSlot.Init();

        UI_EventHandler evt = doSmelt.GetComponent<UI_EventHandler>();
        evt._OnClick += (PointerEventData p)=> { CheckCanSmelt(); };

        evt = close.GetComponent<UI_EventHandler>();
        evt._OnClick += Close;

        evt = back.GetComponent<UI_EventHandler>();
        evt._OnEnter += (PointerEventData p) => { Managers.Game.isHandleUI = true; Managers.Game.mouse.CursorType = Define.CursorType.Normal; };
        evt._OnExit += (PointerEventData p) => { Managers.Game.isHandleUI = false; Managers.Inven.CheckHotBarChoice(); };

        SetData();  
        
        explain.SetActive(false);
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (!_init)
            return;
        Managers.Inven.inventoryUI.gameObject.SetActive(true);
        FillCharcoal();
    }

    void SetData()
    {
        smeltSlot.GetComponentInChildren<UI_Item>().slotInfo = Managers.Inven.smeltSlotInfo;
        smeltSlot.GetComponentInChildren<UI_Item>().Init();

        grillingSlot.GetComponentInChildren<UI_Item>().slotInfo = Managers.Inven.grillingSlotInfo;
        grillingSlot.GetComponentInChildren<UI_Item>().Init();
    }



    void CheckCanSmelt()
    {
        if (isSmelting)
            return;
        SlotInfo slotInfo = grillingSlot.GetComponentInChildren<UI_Item>().slotInfo;
        if (slotInfo != null)
        {
            if (!slotInfo.itemInfo.canSmelt)
            {
                Debug.Log("제련 할 수 있는 아이템이 아이템이 아닙니다");
                return;
            }
        }
        else
        { 
            Debug.Log("슬롯에 제련 할 아이템이 없습니다");
            return;
        }
        if(charcoalSlot.charcoalCount <= 0)
        {
            Debug.Log("석탄이 부족합니다");
            return;
        }
        slotInfo = smeltSlot.GetComponentInChildren<UI_Item>().slotInfo;
        if (slotInfo.itemInfo != null)
        {
            if (slotInfo.itemInfo.itemType == Define.ItemType.Tool)
            {
                Debug.Log("제련 후 아이템 슬롯에 공간이 없습니다");
                return;
            }
            else if(slotInfo.count > slotInfo.itemInfo.maxAmount)
            {
                Debug.Log("제련 후 아이템 슬롯에 공간이 없습니다");
                return;
            }
        }
        furnanace.StartSmelt();
    }

    public void FinishSmelt()
    {
        UI_Item grillItem = grillingSlot.GetComponentInChildren<UI_Item>();
        UI_Item smeltItem = smeltSlot.GetComponentInChildren<UI_Item>();

        smeltItem.slotInfo.itemInfo = grillItem.slotInfo.itemInfo.smelt;
        smeltItem.slotInfo.count++;
        charcoalSlot.charcoalCount--;

        grillItem.slotInfo.count -= 1;
        if (grillItem.slotInfo.count <= 0)
        {
            grillItem.MakeEmptySlot();
        }

        charcoalSlot.SetSlot();
        grillItem.SetInfo();
        smeltItem.SetInfo();
    }

    void FillCharcoal()
    {
        int totalCount = 0;
        for (int i = 0; i < Managers.Inven.inventoryUI.slotList.Count; i++)
        {
            SlotInfo slotInfo  = Managers.Inven.inventoryUI.slotList[i].itemUI.slotInfo;
            if (slotInfo.itemInfo != null)
            {
                if (slotInfo.itemInfo.idName == "Coal")
                {
                    totalCount += slotInfo.count;
                    if (totalCount > slotInfo.itemInfo.maxAmount)
                    {
                        Managers.Inven.AddItems(slotInfo.itemInfo.idName,totalCount - slotInfo.itemInfo.maxAmount);
                        charcoalSlot.charcoalCount = slotInfo.itemInfo.maxAmount;
                        break;
                    }
                    Managers.Inven.inventoryUI.slotList[i].itemUI.MakeEmptySlot();
                    charcoalSlot.charcoalCount = totalCount;
                }
            }
            else continue;
        }
        charcoalSlot.SetExistSlot();
        charcoalSlot.SetSlot();
    }

    void ReturnCoal()
    {
        if (isSmelting)
            return;
        if (charcoalSlot.charcoalCount != 0)
        {
            Managers.Inven.AddItems("Coal", charcoalSlot.charcoalCount);
            charcoalSlot.charcoalCount = 0;
            charcoalSlot.SetEmptySlot();
         }
    }

    void Close(PointerEventData p)
    {
        ReturnCoal();
        gameObject.SetActive(false);
    }
}
