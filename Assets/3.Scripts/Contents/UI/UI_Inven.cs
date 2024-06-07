using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Inven : UI_Base
{
    List<GameObject> keys = new List<GameObject>();

    GameObject back;
    GameObject grid;
    GameObject hide;
    Vector3 startPos;

    enum GameObjects 
    {
        Background,
        Grid,
        Hide
    }

    bool _init;

    public override void Init()
    {
        if (_init)
            return;

        _init = true;
        Bind<GameObject>(typeof(GameObjects));
        back = Get<GameObject>((int)GameObjects.Background);
        grid = Get<GameObject>((int)GameObjects.Grid);
        hide = Get<GameObject>((int)GameObjects.Hide);
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
        GetData();
        MakeKeys();

        gameObject.SetActive(false);
    }

    void GetData()
    {
        for (int i = 0; i < Managers.Inven.inven_itemInfo.Length; i++)
        {
            Managers.Inven.inven_itemInfo[i] = new InvenManager.ItemInfo(1,10);
        }
    }

    void MakeKeys()
    {
        for (int i = 0; i < Managers.Inven.inven_itemInfo.Length; i++)
        {
            GameObject go = Instantiate(Resources.Load<GameObject>("UI/UI_Inven/UI_Inven_Key"), grid.transform);
            keys.Add(go);
            go.GetComponent<UI_Inven_Key>().Init();
            go.GetComponent<UI_Inven_Key>().keyId = i;
            go.GetComponent<UI_Inven_Key>().SetIcon();
        }
    }

    public void SetKeys(int i)
    {
        keys[i].GetComponent<UI_Inven_Key>().SetIcon();
    }

    public void Set_Inven_Info(int key_index, int id, int count)
    {
        if (id == 0)
        {
            keys[key_index].GetComponent<UI_Inven_Key>().EmptyKey();
            return;
        }

        Item item = Resources.Load<GameObject>($"Prefabs/Items/{id}").GetComponent<Item>(); //id에 따른 아이템 정보

        Managers.Inven.inven_itemInfo[key_index].id = id;
        Managers.Inven.inven_itemInfo[key_index].itemType = item.itemType;

        if(count > 99)
        {
            Managers.Inven.AddItem(id,count - 99);
            count = 99;
        }
        Managers.Inven.inven_itemInfo[key_index].count = count;
        Managers.Inven.inven_itemInfo[key_index].icon = item.itemIcon;
        if (Managers.Inven.inven_itemInfo[key_index].itemType == Define.ItemType.Building)   //건설 아이템은 타일을 따로 가지고 있는다
            Managers.Inven.inven_itemInfo[key_index].tile = item.tile;
        Managers.Inven.inven_itemInfo[key_index].keyType = Define.KeyType.Exist;
        SetKeys(key_index);
    }

   
}
