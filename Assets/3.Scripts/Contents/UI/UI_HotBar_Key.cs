using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static InvenManager;
using static Managers;

public class UI_HotBar_Key : UI_Base
{
    public int keyId;
    ItemInfo hotBar;

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
            {
                Managers.Game.mouse.CursorType = Define.CursorType.UI;
                if (Managers.Inven.hotBar_itemInfo[keyId].keyType == Define.KeyType.Empty)
                    return;
                //Managers.Inven.inven.explain.SetActive(true);
                //Managers.Inven.inven.Set_Explain(Managers.Inven.hotBar_itemInfo[keyId].explain, Managers.Inven.hotBar_itemInfo[keyId].itemName);
                //Managers.Inven.inven.explain.GetComponent<RectTransform>().anchoredPosition = transform.position;
            }
            
        };
        evt._OnExit += (PointerEventData p) => 
        {
            if (Game.mouse.CursorType == Define.CursorType.Drag)
            {
                //Inven.changeSpot.invenType = Define.InvenType.None;
                //Managers.Inven.inven.explain.SetActive(false);
            }
            else
            {
                Managers.Game.mouse.CursorType = Define.CursorType.Normal;
                Inven.Set_HotBar_Choice(); 
            }
        };
        evt._OnDown += (PointerEventData p) => 
        {
            if (Inven.hotBar_itemInfo[keyId].keyType == Define.KeyType.Empty)
                return;
            if (keyId != Inven.hotBar_itemInfo.Length - 1)
            {
                Inven.changeSpot.index = keyId;
                Inven.changeSpot.invenType = Define.InvenType.HotBar;
            }
            Game.mouse.CursorType = Define.CursorType.Drag;
            Game.mouse.Set_Mouse_ItemIcon_HotBar(icon,count);
        };
        evt._OnUp += (PointerEventData p) => 
        {
            if (Game.mouse.CursorType != Define.CursorType.Drag)
                return;

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
            Game.mouse.CursorType = Define.CursorType.UI;
        };
    }


    public void SetIcon()
    {
        hotBar = Inven.hotBar_itemInfo[keyId];
        if (hotBar.keyType == Define.KeyType.Empty)
        {
            EmptyKey();
            return;
        }
        if(hotBar.itemInfo.itemType == Define.ItemType.Tower)
        {
            SetTowerIcon();
            return;
        }
        count.gameObject.SetActive(true);
        icon.gameObject.SetActive(true);
        if(hotBar.itemInfo.itemType == Define.ItemType.Tool || hotBar.itemInfo.itemType == Define.ItemType.Tower)
            count.gameObject.SetActive(false);
        icon.sprite = hotBar.itemInfo.itemIcon;

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
        hotBar.itemInfo = null;
        hotBar.keyType = Define.KeyType.Empty;
        hotBar.count = 0;
    }

    public void HideIcon()
    {
        icon.gameObject.SetActive(false);
        count.gameObject.SetActive(false);
    }
    public void ShowIcon()
    {
        icon.gameObject.SetActive(true);
        if(hotBar.itemInfo.itemType != Define.ItemType.Tool)
            count.gameObject.SetActive(true);
    }
    public void SetTowerIcon()
    {
        if (Inven.hotBar_itemInfo[Inven.hotBar_itemInfo.Length - 1].keyType == Define.KeyType.Empty)
        {
            icon.gameObject.SetActive(false);
            count.gameObject.SetActive(false);
            return;
        }
        icon.gameObject.SetActive(true);
        icon.sprite = Managers.Game.tower.GetComponent<SpriteRenderer>().sprite;
    }

    Define.DropType Drop()
    {
        if (Inven.changeSpot.invenType == Define.InvenType.Inven)
        {
            if (Inven.inven_itemInfo[Inven.changeSpot.index].itemInfo == null)
                return Define.DropType.Move;
            else if (-1 == Inven.changeSpot.index)
                return Define.DropType.Return;
            else if (Inven.inven_itemInfo[Inven.changeSpot.index].itemInfo.idName == Inven.hotBar_itemInfo[keyId].itemInfo.idName)
                return Define.DropType.Add;
        }
        else
        {
            if (Inven.hotBar_itemInfo[Inven.changeSpot.index].itemInfo == null)
                return Define.DropType.Move;
            else if (-1 == Inven.changeSpot.index || keyId == Inven.changeSpot.index)
                return Define.DropType.Return;
            else if (Inven.hotBar_itemInfo[Inven.changeSpot.index].itemInfo.idName == Inven.hotBar_itemInfo[keyId].itemInfo.idName)
                return Define.DropType.Add;
        }

        return Define.DropType.Change;
    }

    public void ChangeItemSpot()
    {
        //키 자신의 값
        Item item = hotBar.itemInfo;
        int count = Inven.hotBar_itemInfo[keyId].count;
        if(Inven.changeSpot.invenType == Define.InvenType.Inven)
        {
            Inven.hotBar.Set_HotBar_Info(keyId, Inven.inven_itemInfo[Inven.changeSpot.index].count, Inven.inven_itemInfo[Inven.changeSpot.index].itemInfo);
            Inven.inven.Set_Inven_Info(Inven.changeSpot.index,count,item);
        }
        else
        {
            Inven.hotBar.Set_HotBar_Info(keyId, Inven.hotBar_itemInfo[Inven.changeSpot.index].count, Inven.hotBar_itemInfo[Inven.changeSpot.index].itemInfo);
            Inven.hotBar.Set_HotBar_Info(Inven.changeSpot.index, count, item);
        }
    }

    void MoveItemSpot()
    {
        Item item = hotBar.itemInfo;
        int count = Inven.hotBar_itemInfo[keyId].count;
        if (Inven.changeSpot.invenType == Define.InvenType.Inven)
        {
            Inven.hotBar.Set_HotBar_Info(keyId, 0);
            Inven.inven.Set_Inven_Info(Inven.changeSpot.index, count,item);
        }
        else
        {
            Inven.hotBar.Set_HotBar_Info(keyId, 0);
            Inven.hotBar.Set_HotBar_Info(Inven.changeSpot.index, count, item);
        }
    }

    void CombineItem()
    {
        Item item = hotBar.itemInfo;
        int count = Inven.hotBar_itemInfo[keyId].count;
        if (Inven.changeSpot.invenType == Define.InvenType.Inven)
        {
            Inven.inven.Set_Inven_Info(keyId, 0);
            Inven.inven.Set_Inven_Info(Inven.changeSpot.index, count + Inven.inven_itemInfo[Inven.changeSpot.index].count,item);
        }
        else
        {
            Inven.inven.Set_Inven_Info(keyId, 0);
            Inven.hotBar.Set_HotBar_Info(Inven.changeSpot.index, count + Inven.hotBar_itemInfo[Inven.changeSpot.index].count,item);
        }
    }
}
