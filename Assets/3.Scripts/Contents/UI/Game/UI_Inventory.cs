using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Inventory : UI_Base
{
    public AssetReferenceT<AudioClip> showSoundAsset;
    public AssetReferenceT<AudioClip> hideSoundAsset;

    public AssetReferenceGameObject invenSlotAsset;
    public AssetReferenceGameObject produceUIAsset;

    AudioClip showSound;
    AudioClip hideSound;

    [HideInInspector]
    public UI_InventorySlot[] slotList;

    GameObject grid;
    GameObject hide;
    [HideInInspector]
    public RectTransform back;
    [HideInInspector]
    public GameObject explain;
    [HideInInspector]
    public Text coin;

    public bool canAbandon;

    Vector3 startPos;

    public override void Init()
    {
        if (_init)
            return;


        back = Util.FindChild<RectTransform>(gameObject, "Background", true);
        grid = Util.FindChild(gameObject, "Grid", true);
        hide = Util.FindChild(gameObject, "Hide", true);
        coin = Util.FindChild<Text>(gameObject, "Coin", true);
        explain = Util.FindChild(gameObject, "Explain_Inven", true);

        UI_EventHandler evt = back.GetComponent<UI_EventHandler>();
        evt._OnDrag += (PointerEventData p) =>
        {
            back.transform.position = new Vector3(Camera.main.ScreenToWorldPoint(p.position).x + startPos.x, Camera.main.ScreenToWorldPoint(p.position).y + startPos.y);

            float x = Mathf.Clamp(back.anchoredPosition.x,-640,680);
            float y = Mathf.Clamp(back.anchoredPosition.y,-60,110);
            back.anchoredPosition = new Vector2(x, y);
        };
        evt._OnDown += (PointerEventData p) =>
        {
            startPos = new Vector3(back.transform.position.x - Camera.main.ScreenToWorldPoint(p.position).x, back.transform.position.y - Camera.main.ScreenToWorldPoint(p.position).y);
        };
        evt._OnEnter += (PointerEventData p) =>
        {
            if (Managers.Game.mouse.CursorType == Define.CursorType.Drag)
            {
                InvenManager.canAbandon = false;
                return; 
            }
            Managers.Game.mouse.CursorType = Define.CursorType.UI;
        };
        evt._OnExit += (PointerEventData p) => 
        {
            if (Managers.Game.mouse.CursorType == Define.CursorType.Drag)
            {
                InvenManager.canAbandon = true;
                return; 
            }
            Managers.Inven.CheckHotBarChoice();
        };

        evt = hide.GetComponent<UI_EventHandler>();
        evt._OnClick += (PointerEventData p) => { gameObject.SetActive(false); Managers.Game.isHandleUI = false; };

        showSoundAsset.LoadAssetAsync().Completed += (clip) =>
        {
            showSound = clip.Result;
        };
        hideSoundAsset.LoadAssetAsync().Completed += (clip) =>
        {
            hideSound = clip.Result;
        };

        GetComponent<Canvas>().worldCamera = Camera.main;

        MakeKeys();

        SetCoin();

        explain.SetActive(false);
        gameObject.SetActive(false);

        _init = true;
    }

    private void OnEnable()
    {
        if (_init)
        {
            Managers.Sound.Play(Define.Sound.Effect, showSound);
            Managers.UI.PopUIList.Add(gameObject);
        }
    }

    private void OnDisable()
    {
        if (_init)
        {
            explain.SetActive(false);
            Managers.Sound.Play(Define.Sound.Effect, hideSound);
            Managers.UI.PopUIList.Remove(gameObject);
        }
    }

    void OnDisappear()
    {
        gameObject.SetActive(false);
    }

    void MakeKeys()
    {
        slotList = new UI_InventorySlot[24];
        invenSlotAsset.LoadAssetAsync().Completed += (slot) => 
        {
            for (int i = 0; i < 24; i++)
            {
                UI_InventorySlot go = Instantiate(slot.Result, grid.transform).GetComponent<UI_InventorySlot>();
                slotList[i] = go;
                go.inven = this;
                go.Init();
                slotList[i].itemUI.slotInfo = Managers.Inven.inventorySlotInfo[i];
                go.GetComponentInChildren<UI_Item>().Init();
            }
        };
        Managers.Inven.Coin = 10000;
    }

    public void SetCoin()
    {
        coin.text = $"ÄÚÀÎ : {Managers.Inven.Coin}";
    }
}
