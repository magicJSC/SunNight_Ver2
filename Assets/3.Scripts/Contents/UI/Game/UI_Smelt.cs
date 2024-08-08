using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using static StorageManager;

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
    RectTransform back;

    Vector3 startPos;

    [HideInInspector]
    public Image timer;
    [HideInInspector]
    public GameObject explain;

    [HideInInspector]
    public FurnanceController furnanace;
    public static bool isSmelting;

    private SlotInfo _slotInfo;

    ItemSO coalSO;

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
        back = Util.FindChild<RectTransform>(gameObject,"Background",true);
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
        evt._OnDrag += (PointerEventData p) =>
        {
            back.transform.position = startPos + Input.mousePosition;
            float x = Mathf.Clamp(back.anchoredPosition.x, -660, 660);
            float y = Mathf.Clamp(back.anchoredPosition.y, -240, 240);
            back.anchoredPosition = new Vector2(x, y);
        };
        evt._OnDown += (PointerEventData p) => { startPos = back.transform.position - Input.mousePosition; };

        SetData();

        coalSO = Resources.Load<ItemSO>("Prefabs/Items/Coal");

        Managers.Game.tower.forceInstallEvent -= CancelSmelt;
        Managers.Game.tower.forceInstallEvent += CancelSmelt;

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

    private void OnDisable()
    {
        if (!_init)
            return;
        explain.SetActive(false);
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
        _slotInfo = grillingSlot.GetComponentInChildren<UI_Item>().slotInfo;
        if (_slotInfo != null)
        {
            if (_slotInfo.itemInfo.smelt == null)
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
        _slotInfo = smeltSlot.GetComponentInChildren<UI_Item>().slotInfo;
        if (_slotInfo.itemInfo != null)
        {
            if (_slotInfo.itemInfo.itemType == Define.ItemType.Tool)
            {
                Debug.Log("제련 후 아이템 슬롯에 공간이 없습니다");
                return;
            }
            else if(_slotInfo.count > _slotInfo.itemInfo.maxAmount)
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
        for (int i = 0; i < Managers.Inven.inventoryUI.slotList.Length; i++)
        {
            _slotInfo  = Managers.Inven.inventoryUI.slotList[i].itemUI.slotInfo;
            if (_slotInfo.itemInfo != null)
            {
                if (_slotInfo.itemInfo == coalSO)
                {
                    totalCount += _slotInfo.count;
                    if (totalCount > _slotInfo.itemInfo.maxAmount)
                    {
                        Managers.Inven.AddItems(_slotInfo.itemInfo,totalCount - _slotInfo.itemInfo.maxAmount);
                        charcoalSlot.charcoalCount = _slotInfo.itemInfo.maxAmount;
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

    void CancelSmelt()
    {
        ReturnItems();
    }

    void ReturnItems()
    {
        if (charcoalSlot.charcoalCount != 0)
        {
            Managers.Inven.AddItems(coalSO, charcoalSlot.charcoalCount);
            charcoalSlot.charcoalCount = 0;
            charcoalSlot.SetEmptySlot();
        }
        _slotInfo = grillingSlot.itemUI.slotInfo;
        if(_slotInfo.itemInfo != null)
        {
            Managers.Inven.AddItems(_slotInfo.itemInfo, _slotInfo.count);
            grillingSlot.itemUI.MakeEmptySlot();
        }
        _slotInfo = smeltSlot.itemUI.slotInfo;
        if (_slotInfo.itemInfo != null)
        {
            Managers.Inven.AddItems(_slotInfo.itemInfo, _slotInfo.count);
            smeltSlot.itemUI.MakeEmptySlot();
        }
    }

    void Close(PointerEventData p)
    {
        if (!isSmelting)
            ReturnItems();
        gameObject.SetActive(false);
    }
}
