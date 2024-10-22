using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using static InvenManager;

/// <summary>
/// 제련 할 때 생성되는 UI입니다
/// </summary>
public class UI_Smelt : UI_Base
{
    [HideInInspector]
    public UI_GrillingSlot grillingSlot;
    [HideInInspector]
    public UI_SmeltSlot smeltSlot;
    [HideInInspector]
    public UI_CharcoalSlot charcoalSlot;

    GameObject close;
    GameObject doSmelt;
    RectTransform back;

    Vector3 startPos;

    [HideInInspector]
    public Image timer;
    [HideInInspector]
    public GameObject explain;

    [HideInInspector]
    public FurnanceController furnanace;
    public bool isSmelting;

    private SlotInfo _slotInfo;


  
    public override void Init()
    {
        if (_init)
            return;

        _init = true;


        grillingSlot = Util.FindChild<UI_GrillingSlot>(gameObject, "GrillingSlot",true);
        close = Util.FindChild(gameObject, "CloseSmelt", true);
        back = Util.FindChild<RectTransform>(gameObject,"Background",true);
        smeltSlot = Util.FindChild<UI_SmeltSlot>(gameObject, "SmeltSlot", true);
        doSmelt = Util.FindChild(gameObject, "DoSmelt", true);
        charcoalSlot = Util.FindChild<UI_CharcoalSlot>(gameObject, "CharcoalSlot", true);
        explain = Util.FindChild(gameObject, "ExplainSmelt", true);
        timer = Util.FindChild<Image>(gameObject, "Timer", true);

        smeltSlot.smeltUI = this;

        grillingSlot.smeltUI = this;

        charcoalSlot.smeltUI = this;

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

        furnanace.SetTimer();

        explain.SetActive(false);
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (!_init)
            return;
        Managers.Inven.inventoryUI.gameObject.SetActive(true);
        Managers.UI.PopUIList.Add(gameObject);
    }

    private void OnDisable()
    {
        if (!_init)
            return;
        explain.SetActive(false);
        Managers.UI.PopUIList.Remove(gameObject);
    }

    void CheckCanSmelt()
    {
        if (isSmelting)
            return;
        _slotInfo = grillingSlot.GetComponentInChildren<UI_Item>().slotInfo;
        if (_slotInfo.keyType == Define.KeyType.Empty)
            return;
        if (!_slotInfo.itemInfo.canSmelt)
        {
            Debug.Log("제련 할 수 있는 아이템이 아이템이 아닙니다");
            return;
        }
        if (isSmelting)
            return;
        if(charcoalSlot.itemUI.slotInfo.count <= 0)
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
        UI_Item coalItem = charcoalSlot.GetComponentInChildren<UI_Item>();

        smeltItem.slotInfo.itemInfo = grillItem.slotInfo.itemInfo.smeltItem;
        smeltItem.slotInfo.count++;
        charcoalSlot.itemUI.slotInfo.count--;

        isSmelting = false;

        grillItem.slotInfo.count -= 1;
        if (grillItem.slotInfo.count <= 0)
        {
            grillItem.MakeEmptySlot();
        }

        coalItem.SetInfo();
        grillItem.SetInfo();
        smeltItem.SetInfo();

        CheckCanSmelt();
    }

    void Close(PointerEventData p)
    {
        gameObject.SetActive(false);
    }
}
