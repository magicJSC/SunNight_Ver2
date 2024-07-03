using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using static StorageManager;

public class UI_Inventory : UI_Base
{
    public List<UI_InventorySlot> slotList = new List<UI_InventorySlot>();

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

        _init = true;
        Bind<GameObject>(typeof(GameObjects));
        back = Get<GameObject>((int)GameObjects.Background);
        grid = Get<GameObject>((int)GameObjects.Grid);
        hide = Get<GameObject>((int)GameObjects.Hide);
        produce = Get<GameObject>((int)GameObjects.Produce);
        coin = Get<GameObject>((int)GameObjects.Coin).GetComponent<Text>();
        explain = Get<GameObject>((int)GameObjects.Explain_Inven);
        produceUI = Get<GameObject>((int)GameObjects.UI_Produce).GetComponent<UI_Produce>();
       
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
                return;
            Managers.Game.isHandleUI = true;
        };
        evt._OnExit += (PointerEventData p) => 
        {
            Managers.Game.isHandleUI = false;
            if (Managers.Game.mouse.CursorType == Define.CursorType.Drag)
                return;
            Managers.Inven.CheckHotBarChoice();
        };

        evt = hide.GetComponent<UI_EventHandler>();
        evt._OnClick += (PointerEventData p) => { gameObject.SetActive(false); Managers.Game.isHandleUI = false; };



        evt = produce.GetComponent<UI_EventHandler>();
        evt._OnClick += ShowProduceUI;

        GetData();
        MakeKeys();

        SetCoin();

        explain.SetActive(false);
        gameObject.SetActive(false);
    }

    void GetData()
    {
        Managers.Inven.inventorySlotInfo[0] = new SlotInfo(5, "Branch");
        Managers.Inven.inventorySlotInfo[1] = new SlotInfo(5, "Bone");
        Managers.Inven.inventorySlotInfo[2] = new SlotInfo(5, "Coal");
        Managers.Inven.inventorySlotInfo[3] = new SlotInfo(5, "IronOre");
        Managers.Inven.inventorySlotInfo[4] = new SlotInfo(1, "Furnance");
        for (int i = 5; i < Managers.Inven.inventorySlotInfo.Length; i++)
        {
            Managers.Inven.inventorySlotInfo[i] = new SlotInfo(0);
        }
    }

    void MakeKeys()
    {
        for (int i = 0; i < Managers.Inven.inventorySlotInfo.Length; i++)
        {
            UI_InventorySlot go = Instantiate(Resources.Load<GameObject>("UI/UI_Inven_Slot"), grid.transform).GetComponent<UI_InventorySlot>();
            slotList.Add(go);
            go.inven = this;
            go.Init();
            go.GetComponentInChildren<UI_Item>().slotInfo = Managers.Inven.inventorySlotInfo[i];
            go.GetComponentInChildren<UI_Item>().Init();
        }
    }

    public void SetCoin()
    {
        coin.text = "ÄÚÀÎ : " + Managers.Inven.Coin.ToString();
    }

    void ShowProduceUI(PointerEventData p)
    {
        if (produceUI.gameObject.activeSelf)
            return;
        produceUI.gameObject.SetActive(true);
        RectTransform r = produceUI.back.GetComponent<RectTransform>();
        RectTransform bb = back.GetComponent<RectTransform>();
        r.anchoredPosition = new Vector2(bb.anchoredPosition.x - 615, bb.anchoredPosition.y - 20);
        produceUI.Set_Position();
    }
}
