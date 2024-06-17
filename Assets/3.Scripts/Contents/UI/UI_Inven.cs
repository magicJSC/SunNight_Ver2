using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using static InvenManager;

public class UI_Inven : UI_Base
{
    List<GameObject> keys = new List<GameObject>();

    GameObject back;
    GameObject grid;
    GameObject hide;
    GameObject produce;
    GameObject produceUI;
    public GameObject explain;

    public Text coin;
   

    Vector3 startPos;

    enum GameObjects 
    {
        Background,
        Grid,
        Hide,
        Produce,
        UI_Produce,
        Coin,
        Explain_Inven
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
        produceUI = Get<GameObject>((int)GameObjects.UI_Produce);
        coin = Get<GameObject>((int)GameObjects.Coin).GetComponent<Text>();
        explain = Get<GameObject>((int)GameObjects.Explain_Inven);
        

        GetComponent<Canvas>().worldCamera = Camera.main;

        UI_EventHandler evt = back.GetComponent<UI_EventHandler>();
        evt._OnDrag += (PointerEventData p) => { back.transform.position = new Vector3(Camera.main.ScreenToWorldPoint(p.position).x + startPos.x, Camera.main.ScreenToWorldPoint(p.position).y + startPos.y);};
        evt._OnDown += (PointerEventData p) => { startPos = new Vector3(back.transform.position.x - Camera.main.ScreenToWorldPoint(p.position).x, back.transform.position.y - Camera.main.ScreenToWorldPoint(p.position).y); };
        evt._OnEnter += (PointerEventData p) =>
        {
            if (Managers.Game.mouse.CursorType == Define.CursorType.Drag)
                return;
            Managers.Game.mouse.CursorType = Define.CursorType.Normal;
        };
        evt._OnExit += (PointerEventData p) => 
        {
            if (Managers.Game.mouse.CursorType == Define.CursorType.Drag)
                return;
            Managers.Inven.Set_HotBar_Choice();
        };

        evt = hide.GetComponent<UI_EventHandler>();
        evt._OnClick += (PointerEventData p) => { gameObject.SetActive(false); };



        evt = produce.GetComponent<UI_EventHandler>();
        evt._OnClick += (PointerEventData p) => { produceUI.SetActive(true); };

        GetData();
        MakeKeys();

        SetCoin();

        produceUI.SetActive(false);
        explain.SetActive(false);
        gameObject.SetActive(false);
    }

    void GetData()
    {
        Managers.Inven.inven_itemInfo[0] = new InvenManager.ItemInfo(5,Define.InvenType.Inven, "Branch");
        Managers.Inven.inven_itemInfo[1] = new InvenManager.ItemInfo(5,Define.InvenType.Inven, "Bone");
        for (int i = 2; i < Managers.Inven.inven_itemInfo.Length; i++)
        {
            Managers.Inven.inven_itemInfo[i] = new InvenManager.ItemInfo(0, Define.InvenType.Inven);
        }
    }

    void MakeKeys()
    {
        for (int i = 0; i < Managers.Inven.inven_itemInfo.Length; i++)
        {
            GameObject go = Instantiate(Resources.Load<GameObject>("UI/UI_Inven/UI_Inven_Key"), grid.transform);
            keys.Add(go);
            go.GetComponent<UI_Inven_Key>().inven = this;
            go.GetComponent<UI_Inven_Key>().Init();
            go.GetComponent<UI_Inven_Key>().keyId = i;
            go.GetComponent<UI_Inven_Key>().SetIcon();
        }
    }

    public void SetKeys(int i)
    {
        keys[i].GetComponent<UI_Inven_Key>().SetIcon();
    }

    public void Set_Inven_Info(int key_index,int count,Item item =null)
    {
        Managers.Inven.inven_itemInfo[key_index].itemInfo = item;
        if (item == null || count == 0)
        {
            keys[key_index].GetComponent<UI_Inven_Key>().EmptyKey();
            return;
        }
        if (count > 99)
        {
            Managers.Inven.AddItem(item.itemName,count - 99);
            count = 99;
        }
        Managers.Inven.inven_itemInfo[key_index].count = count;
        if (item.itemType == Define.ItemType.Building)   //건설 아이템은 타일을 따로 가지고 있는다
            Managers.Inven.inven_itemInfo[key_index].tile = Resources.Load<TileBase>($"TileMap/{item.itemName}");
        Managers.Inven.inven_itemInfo[key_index].keyType = Define.KeyType.Exist;
        SetKeys(key_index);
    }

    public void SetCoin()
    {
        coin.text = "코인 : " + Managers.Inven.Coin.ToString();
    }

   
}
