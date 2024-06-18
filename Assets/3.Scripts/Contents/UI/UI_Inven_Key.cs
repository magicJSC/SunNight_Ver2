using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static InvenManager;

public class UI_Inven_Key : UI_Base
{
    public UI_Inven inven;
    public int keyId;
    public ItemInfo invenInfo;

    Image icon;
    Text count;
    enum Images 
    {
        Icon
    }

    enum Texts 
    {
        Count
    }

    GameObject explain;
    Text explainText;
    Text nameText;

    public new void Init()
    {
        invenInfo = Managers.Inven.inven_itemInfo[keyId];

        Bind<Image>(typeof(Images));
        Bind<Text>(typeof(Texts));
        icon = Get<Image>((int)Images.Icon);
        count = Get<Text>((int)Texts.Count);

        explain = inven.explain;
        explainText = Util.FindChild(explain, "ExplainText", true).GetComponent<Text>();
        nameText = Util.FindChild(explain, "NameText", true).GetComponent<Text>();

        UI_EventHandler evt = GetComponent<UI_EventHandler>();
        evt._OnDown += (PointerEventData p) => 
        {
            if (Managers.Inven.inven_itemInfo[keyId].keyType == Define.KeyType.Empty)
                return;
            Managers.Inven.changeSpot.index = keyId;
            Managers.Inven.changeSpot.invenType = Define.InvenType.Inven;
            Managers.Game.mouse.CursorType = Define.CursorType.Drag;
            Managers.Game.mouse.Set_Mouse_ItemIcon_Inven(icon,count);
        };
        evt._OnEnter += (PointerEventData p) =>
        {
            if (Managers.Game.mouse.CursorType == Define.CursorType.Drag)
            {
                if (keyId != Managers.Inven.inven_itemInfo.Length - 1)
                {
                    Managers.Inven.changeSpot.index = keyId;
                    Managers.Inven.changeSpot.invenType = Define.InvenType.Inven;
                }
            }
            else
            {
                if (Managers.Inven.inven_itemInfo[keyId].keyType == Define.KeyType.Empty)
                    return;
                inven.explain.SetActive(true);
                Set_Explain();
            }
        };
        evt._OnUp += (PointerEventData p) => 
        {
            if (Managers.Game.mouse.CursorType != Define.CursorType.Drag)
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
            Managers.Game.mouse.CursorType = Define.CursorType.Normal;
        };
        evt._OnExit += (PointerEventData p) =>
        {
            if (Managers.Game.mouse.CursorType == Define.CursorType.Drag)
                Managers.Inven.changeSpot.index = -1;
            explain.SetActive(false);
        };
    }

    public void Set_Explain()
    {
        int x, y;
        if (inven.back.GetComponent<RectTransform>().anchoredPosition.x < -290)
            x = -25;
        else
            x = -545;
        if (inven.back.GetComponent<RectTransform>().anchoredPosition.y > -40)
            y = 180;
        else
            y = 565;


        explain.GetComponent<RectTransform>().anchoredPosition = GetComponent<RectTransform>().anchoredPosition + new Vector2(x, y);

        explainText.text = invenInfo.itemInfo.explain;
        nameText.text = invenInfo.itemInfo.itemName;
    }

    public void SetIcon()
    {
        invenInfo = Managers.Inven.inven_itemInfo[keyId];
        if (invenInfo.keyType == Define.KeyType.Empty)
        {
            EmptyKey();
            return;
        }

        icon.sprite = invenInfo.itemInfo.itemIcon; 
        count.text = invenInfo.count.ToString();

        ShowIcon();
    }

    public void EmptyKey()  //키 비어있게 만들기
    {
        HideIcon();
        invenInfo.itemInfo = null;
        invenInfo.keyType = Define.KeyType.Empty;
        invenInfo.count = 0;
    }

    public void HideIcon()
    {
        icon.gameObject.SetActive(false);
        count.gameObject.SetActive(false);
    }

    public void ShowIcon()
    {
        count.gameObject.SetActive(true);
        icon.gameObject.SetActive(true);
        if (invenInfo.itemInfo.itemType == Define.ItemType.Tool)
            count.gameObject.SetActive(false);
    }

    Define.DropType Drop()
    {
        if (Managers.Inven.changeSpot.invenType == Define.InvenType.Inven)
        {
            if (Managers.Inven.inven_itemInfo[Managers.Inven.changeSpot.index].itemInfo == null)
                return Define.DropType.Move;
            else if (-1 == Managers.Inven.changeSpot.index || keyId == Managers.Inven.changeSpot.index)
                return Define.DropType.Return;
            else if (Managers.Inven.inven_itemInfo[Managers.Inven.changeSpot.index].itemInfo.idName == invenInfo.itemInfo.idName)
                return Define.DropType.Add;
        }
        else
        {
            if (Managers.Inven.hotBar_itemInfo[Managers.Inven.changeSpot.index].itemInfo == null)
                return Define.DropType.Move;
            else if (-1 == Managers.Inven.changeSpot.index)
                return Define.DropType.Return;
            else if (Managers.Inven.hotBar_itemInfo[Managers.Inven.changeSpot.index].itemInfo.idName == invenInfo.itemInfo.idName)
                return Define.DropType.Add;
        }

        return Define.DropType.Change;
    }

    public void ChangeItemSpot()
    {
        //키 자신의 값
        Item item = invenInfo.itemInfo;
        int count = Managers.Inven.inven_itemInfo[keyId].count;
        if (Managers.Inven.changeSpot.invenType == Define.InvenType.Inven)
        {
            Managers.Inven.inven.Set_Inven_Info(keyId,Managers.Inven.inven_itemInfo[Managers.Inven.changeSpot.index].count, Managers.Inven.inven_itemInfo[Managers.Inven.changeSpot.index].itemInfo);
            Managers.Inven.inven.Set_Inven_Info(Managers.Inven.changeSpot.index,count,item);
        }
        else
        {
            Managers.Inven.inven.Set_Inven_Info(keyId, Managers.Inven.hotBar_itemInfo[Managers.Inven.changeSpot.index].count, Managers.Inven.hotBar_itemInfo[Managers.Inven.changeSpot.index].itemInfo);
            Managers.Inven.hotBar.Set_HotBar_Info(Managers.Inven.changeSpot.index, count, item);
        }
    }

    void MoveItemSpot()
    {
        Item item = invenInfo.itemInfo;
        int count = Managers.Inven.inven_itemInfo[keyId].count;
        if (Managers.Inven.changeSpot.invenType == Define.InvenType.Inven)
        {
            Managers.Inven.inven.Set_Inven_Info(keyId, 0);
            Managers.Inven.inven.Set_Inven_Info(Managers.Inven.changeSpot.index, count, item);
        }
        else
        {
            Managers.Inven.inven.Set_Inven_Info(keyId, 0);
            Managers.Inven.hotBar.Set_HotBar_Info(Managers.Inven.changeSpot.index, count, item);
        }
    }

    void CombineItem()
    {
        Item item = invenInfo.itemInfo;
        int count = Managers.Inven.inven_itemInfo[keyId].count;
        if (Managers.Inven.changeSpot.invenType == Define.InvenType.Inven)
        {
            Managers.Inven.inven.Set_Inven_Info(keyId, 0);
            Managers.Inven.inven.Set_Inven_Info(Managers.Inven.changeSpot.index, count + Managers.Inven.inven_itemInfo[Managers.Inven.changeSpot.index].count,item);
        }
        else
        {
            Managers.Inven.inven.Set_Inven_Info(keyId, 0);
            Managers.Inven.hotBar.Set_HotBar_Info(Managers.Inven.changeSpot.index, count + Managers.Inven.hotBar_itemInfo[Managers.Inven.changeSpot.index].count,item);
        }
    }
}
