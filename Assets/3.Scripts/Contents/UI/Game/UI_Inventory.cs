using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static StorageManager;

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
    GameObject produce;
    [HideInInspector]
    public GameObject back;
    [HideInInspector]
    public GameObject explain;
    UI_Produce produceUI;
    [HideInInspector]
    public Text coin;

    Text produceText;

    public bool canAbandon;

    Vector3 startPos;

    enum GameObjects 
    {
        Background,
        Grid,
        Hide,
        Produce,
        Coin,
        Explain_Inven,
        UI_Produce
    }


    public override void Init()
    {
        if (_init)
            return;

        Bind<GameObject>(typeof(GameObjects));
        back = Get<GameObject>((int)GameObjects.Background);
        grid = Get<GameObject>((int)GameObjects.Grid);
        hide = Get<GameObject>((int)GameObjects.Hide);
        produce = Get<GameObject>((int)GameObjects.Produce);
        coin = Get<GameObject>((int)GameObjects.Coin).GetComponent<Text>();
        explain = Get<GameObject>((int)GameObjects.Explain_Inven);
        produceUIAsset.LoadAssetAsync().Completed += (obj) => 
        {
            produceUI = Instantiate(obj.Result).GetComponent<UI_Produce>();
        };

        produceText = Util.FindChild(gameObject, "ProduceText", true).GetComponent<Text>();

        UI_EventHandler evt = back.GetComponent<UI_EventHandler>();
        evt._OnDrag += (PointerEventData p) =>
        {
            back.transform.position =startPos+Input.mousePosition;
            float x = Mathf.Clamp(back.GetComponent<RectTransform>().anchoredPosition.x,-640,680);
            float y = Mathf.Clamp(back.GetComponent<RectTransform>().anchoredPosition.y,-60,110);
            back.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
        };
        evt._OnDown += (PointerEventData p) => { startPos = back.transform.position - Input.mousePosition; };
        evt._OnEnter += (PointerEventData p) =>
        {
            if (Managers.Game.mouse.CursorType == Define.CursorType.Drag)
            {
                StorageManager.canAbandon = false;
                return; 
            }
            Managers.Game.mouse.CursorType = Define.CursorType.UI;
        };
        evt._OnExit += (PointerEventData p) => 
        {
            if (Managers.Game.mouse.CursorType == Define.CursorType.Drag)
            {
                StorageManager.canAbandon = true;
                return; 
            }
            Managers.Inven.CheckHotBarChoice();
        };

        evt = hide.GetComponent<UI_EventHandler>();
        evt._OnClick += (PointerEventData p) => { gameObject.SetActive(false); Managers.Game.isHandleUI = false; };

        evt = produce.GetComponent<UI_EventHandler>();
        evt._OnClick += ShowProduceUI;
        evt._OnEnter += (PointerEventData p)=> { produceText.color = Color.red; };
        evt._OnExit += (PointerEventData p) => { produceText.color = Color.white; };

        showSoundAsset.LoadAssetAsync().Completed += (clip) =>
        {
            showSound = clip.Result;
        };
        hideSoundAsset.LoadAssetAsync().Completed += (clip) =>
        {
            hideSound = clip.Result;
        };

        GetData();
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
            produceUI.gameObject.SetActive(false);
            Managers.Sound.Play(Define.Sound.Effect, hideSound);
            Managers.UI.PopUIList.Remove(gameObject);
        }
    }

    void OnDisappear()
    {
        gameObject.SetActive(false);
    }

    void GetData()
    {
        Managers.Inven.inventorySlotInfo[0] = new SlotInfo(6, "Meat");
        Managers.Inven.inventorySlotInfo[1] = new SlotInfo(1,"Bonfire");
        for (int i = 2; i < Managers.Inven.inventorySlotInfo.Length; i++)
        {
            Managers.Inven.inventorySlotInfo[i] = new SlotInfo(0);
        }


    }

    void MakeKeys()
    {
        slotList = new UI_InventorySlot[Managers.Inven.inventorySlotInfo.Length];
        invenSlotAsset.LoadAssetAsync().Completed += (slot) => 
        {
            for (int i = 0; i < Managers.Inven.inventorySlotInfo.Length; i++)
            {
                UI_InventorySlot go = Instantiate(slot.Result, grid.transform).GetComponent<UI_InventorySlot>();
                slotList[i] = go;
                go.inven = this;
                go.Init();
                go.GetComponentInChildren<UI_Item>().slotInfo = Managers.Inven.inventorySlotInfo[i];
                go.GetComponentInChildren<UI_Item>().Init();
            }
        };
        Managers.Inven.Coin = 10000;
    }

    public void SetCoin()
    {
        coin.text = $"ÄÚÀÎ : {Managers.Inven.Coin}";
    }

    void ShowProduceUI(PointerEventData p)
    {
        if (produceUI.gameObject.activeSelf)
        {
            produceUI.gameObject.SetActive(false);
            return;
        }
        produceUI.gameObject.SetActive(true);
        RectTransform r = produceUI.back.GetComponent<RectTransform>();
        RectTransform bb = back.GetComponent<RectTransform>();
        r.anchoredPosition = new Vector2(bb.anchoredPosition.x - 615, bb.anchoredPosition.y - 20);
        produceUI.Set_Position();
    }
}
