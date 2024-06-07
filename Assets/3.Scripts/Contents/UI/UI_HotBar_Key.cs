using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Managers;

public class UI_HotBar_Key : UI_Base
{
    public int keyId;

    Image icon;
    Text count;
    public GameObject choice;
    enum Images
    {
        item,
    }

    enum Texts
    {
        Count
    }

    enum GameObjects
    {
        Choice
    }

    public new void Init()
    {
        Bind<Image>(typeof(Images));
        Bind<Text>(typeof(Texts));
        Bind<GameObject>(typeof(GameObjects));
        icon = Get<Image>((int)Images.item);
        count = Get<Text>((int)Texts.Count);
        choice = Get<GameObject>((int)GameObjects.Choice);
        choice.SetActive(false);

        UI_EventHandler evt = GetComponent<UI_EventHandler>();
        evt._OnEnter += (PointerEventData p) =>
        {
            
            if (Game.mouse.CursorType == Define.CursorType.Drag)
            {
                if (keyId != Inven.hotBar_itemInfo.Length - 1)
                {
                    Inven.changeSpot.index = keyId;
                    Inven.changeSpot.invenType = Define.InvenType.HotBar;
                }
            }
            else
                Game.mouse.CursorType = Define.CursorType.Normal;
            
        };
        evt._OnExit += (PointerEventData p) => 
        {
            if (Game.mouse.CursorType == Define.CursorType.Drag)
                Inven.changeSpot.invenType = Define.InvenType.None;
            else
                Inven.Set_HotBar_Choice();
        };
        evt._OnDown += (PointerEventData p) => 
        {
            if (Inven.hotBar_itemInfo[keyId].keyType == Define.KeyType.Empty)
                return;
            Game.mouse.CursorType = Define.CursorType.Drag;
            Game.mouse.Set_Mouse_ItemIcon(icon,count);
        };
        evt._OnUp += (PointerEventData p) => 
        {
            Define.DropType drop = Drop();
            switch (drop) 
            {
                case Define.DropType.Move:
                    MoveItemSpot();
                    break;
                case Define.DropType.Change:
                    ChangeItemSpot();
                    break;
                case Define.DropType.Add:
                    CombineItem();
                    break;
                case Define.DropType.Return:
                    ShowIcon();
                    break;
            }
            Game.mouse.CursorType = Define.CursorType.Normal;
        };
    }


    public void SetIcon()
    {
        InvenManager.ItemInfo hotBar = Inven.hotBar_itemInfo[keyId];
        if(hotBar.itemType == Define.ItemType.Tower)
        {
            SetTowerIcon();
            return;
        }

        if (hotBar.keyType == Define.KeyType.Empty)
        {
            EmptyKey();
            return;
        }
        count.gameObject.SetActive(true);
        icon.gameObject.SetActive(true);
        if(hotBar.itemType == Define.ItemType.Tool || hotBar.itemType == Define.ItemType.Tower)
            count.gameObject.SetActive(false);
        icon.sprite = hotBar.icon;

        count.text = hotBar.count.ToString();
    }

    public void Choice()
    {
        choice.gameObject.SetActive(true);
    }

    public void UnChoice()
    {
        choice.gameObject.SetActive(false);
    }

    public void EmptyKey()  //키 비어있게 만들기
    {
        HideIcon();
        Inven.hotBar_itemInfo[keyId].keyType = Define.KeyType.Empty;
        Inven.hotBar_itemInfo[keyId].itemType = Define.ItemType.None;
        Inven.hotBar_itemInfo[keyId].count = 0;
        Inven.hotBar_itemInfo[keyId].id = 0;
    }

    public void HideIcon()
    {
        icon.gameObject.SetActive(false);
        count.gameObject.SetActive(false);
    }
    public void ShowIcon()
    {
        icon.gameObject.SetActive(true);
        if(Inven.hotBar_itemInfo[keyId].itemType == Define.ItemType.Tool)
        count.gameObject.SetActive(true);
    }
    public void SetTowerIcon()
    {

        if (Inven.hotBar_itemInfo[Inven.hotBar_itemInfo.Length - 1].keyType == Define.KeyType.Empty)
        {
            icon.gameObject.SetActive(false);
            count.gameObject.SetActive(false);
            Inven.hotBar_itemInfo[Inven.hotBar_itemInfo.Length - 1].itemType = Define.ItemType.None;
            return;
        }
        icon.gameObject.SetActive(true);
        icon.sprite = Inven.GetComponent<SpriteRenderer>().sprite;
    }

    Define.DropType Drop()
    {
        if (Inven.changeSpot.invenType == Define.InvenType.Inven)
        {
            if (Inven.changeSpot.invenType == Define.InvenType.None)
                return Define.DropType.Return;
            else if (Inven.inven_itemInfo[Inven.changeSpot.index].id == Inven.hotBar_itemInfo[keyId].id)
                return Define.DropType.Add;
            else if (Inven.inven_itemInfo[Inven.changeSpot.index].keyType == Define.KeyType.Empty)
                return Define.DropType.Move;
        }
        else
        {
            if (Inven.changeSpot.invenType == Define.InvenType.None)
                return Define.DropType.Return;
            else if (Inven.hotBar_itemInfo[Inven.changeSpot.index].id == Inven.hotBar_itemInfo[keyId].id)
                return Define.DropType.Add;
            else if (Inven.hotBar_itemInfo[Inven.changeSpot.index].keyType == Define.KeyType.Empty)
                return Define.DropType.Move;
        }

        return Define.DropType.Change;
    }

    public void ChangeItemSpot()
    {
        if (Inven.changeSpot.invenType == Define.InvenType.None)
            ShowIcon();
        //키 자신의 값
        int id = Inven.hotBar_itemInfo[keyId].id;
        int count = Inven.hotBar_itemInfo[keyId].count;
        if(Inven.changeSpot.invenType == Define.InvenType.Inven)
        {
            Inven.hotBar.Set_HotBar_Info(keyId, Inven.inven_itemInfo[Inven.changeSpot.index].id, Inven.inven_itemInfo[Inven.changeSpot.index].count);
            Inven.inven.Set_Inven_Info(Inven.changeSpot.index,id,count);
        }
        else
        {
            Inven.hotBar.Set_HotBar_Info(keyId, Inven.hotBar_itemInfo[Inven.changeSpot.index].id, Inven.hotBar_itemInfo[Inven.changeSpot.index].count);
            Inven.hotBar.Set_HotBar_Info(Inven.changeSpot.index, id, count);
        }
    }

    void MoveItemSpot()
    {
        int id = Inven.hotBar_itemInfo[keyId].id;
        int count = Inven.hotBar_itemInfo[keyId].count;
        if (Inven.changeSpot.invenType == Define.InvenType.Inven)
        {
            Inven.hotBar.Set_HotBar_Info(keyId, 0, 0);
            Inven.inven.Set_Inven_Info(Inven.changeSpot.index, id, count);
        }
        else
        {
            Inven.hotBar.Set_HotBar_Info(keyId, 0, 0);
            Inven.hotBar.Set_HotBar_Info(Inven.changeSpot.index, id, count);
        }
    }

    void CombineItem()
    {
        int id = Inven.hotBar_itemInfo[keyId].id;
        int count = Inven.hotBar_itemInfo[keyId].count;
        if (Inven.changeSpot.invenType == Define.InvenType.Inven)
        {
            Inven.inven.Set_Inven_Info(keyId, 0, 0);
            Inven.inven.Set_Inven_Info(Inven.changeSpot.index, id, count + Inven.inven_itemInfo[Inven.changeSpot.index].count);
        }
        else
        {
            Inven.inven.Set_Inven_Info(keyId, 0, 0);
            Inven.hotBar.Set_HotBar_Info(Inven.changeSpot.index, id, count + Inven.hotBar_itemInfo[Inven.changeSpot.index].count);
        }
    }
}
