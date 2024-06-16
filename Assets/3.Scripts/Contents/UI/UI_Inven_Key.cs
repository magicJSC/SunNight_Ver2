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
            Managers.Game.mouse.Set_Mouse_ItemIcon(icon,count);
        };
        evt._OnEnter += (PointerEventData p) =>
        {
            if (Managers.Game.mouse.CursorType == Define.CursorType.Drag)
            {
                if (keyId != Managers.Inven.hotBar_itemInfo.Length - 1)
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
             Managers.Inven.changeSpot.invenType = Define.InvenType.None;
            explain.SetActive(false);
        };
    }

    public void Set_Explain()
    {
        explain.GetComponent<RectTransform>().anchoredPosition = GetComponent<RectTransform>().anchoredPosition + new Vector2(-545, 565);

        //Manager에서 값을 가져오지 말고 ItemInfo를 넣어두자
        //explainText.text = itemInfo.explain;
        //nameText.text = itemInfo.itemName;
        explainText.text = Managers.Inven.inven_itemInfo[keyId].explain;
        nameText.text = Managers.Inven.inven_itemInfo[keyId].itemName;
    }

    public void SetIcon()
    {
        if (Managers.Inven.inven_itemInfo[keyId].keyType == Define.KeyType.Empty)
        {
            EmptyKey();
            return;
        }

        icon.sprite = Managers.Inven.inven_itemInfo[keyId].icon; 
        count.text = Managers.Inven.inven_itemInfo[keyId].count.ToString();

        ShowIcon();
    }

    public void EmptyKey()  //키 비어있게 만들기
    {
        HideIcon();
        Managers.Inven.inven_itemInfo[keyId].idName = "";
        Managers.Inven.inven_itemInfo[keyId].keyType = Define.KeyType.Empty;
        Managers.Inven.inven_itemInfo[keyId].itemType = Define.ItemType.None;
        Managers.Inven.inven_itemInfo[keyId].count = 0;
        Managers.Inven.inven_itemInfo[keyId].id = 0;
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
        if (Managers.Inven.inven_itemInfo[keyId].itemType == Define.ItemType.Tool)
            count.gameObject.SetActive(false);
    }

    Define.DropType Drop()
    {
        if (Managers.Inven.changeSpot.invenType == Define.InvenType.Inven)
        {
            if (Managers.Inven.changeSpot.invenType == Define.InvenType.None || keyId == Managers.Inven.changeSpot.index)
                return Define.DropType.Return;
            else if (Managers.Inven.inven_itemInfo[Managers.Inven.changeSpot.index].idName == Managers.Inven.inven_itemInfo[keyId].idName)
                return Define.DropType.Add;
            else if (Managers.Inven.inven_itemInfo[Managers.Inven.changeSpot.index].keyType == Define.KeyType.Empty)
                return Define.DropType.Move;
        }
        else
        {
            if (Managers.Inven.changeSpot.invenType == Define.InvenType.None)
                return Define.DropType.Return;
            else if (Managers.Inven.hotBar_itemInfo[Managers.Inven.changeSpot.index].idName == Managers.Inven.inven_itemInfo[keyId].idName)
                return Define.DropType.Add;
            else if (Managers.Inven.hotBar_itemInfo[Managers.Inven.changeSpot.index].keyType == Define.KeyType.Empty)
                return Define.DropType.Move;
        }

        return Define.DropType.Change;
    }

    public void ChangeItemSpot()
    {
        //키 자신의 값
        string _name = Managers.Inven.inven_itemInfo[keyId].idName;
        int count = Managers.Inven.inven_itemInfo[keyId].count;
        if (Managers.Inven.changeSpot.invenType == Define.InvenType.Inven)
        {
            Managers.Inven.inven.Set_Inven_Info(keyId,Managers.Inven.inven_itemInfo[Managers.Inven.changeSpot.index].count, Managers.Inven.inven_itemInfo[Managers.Inven.changeSpot.index].idName);
            Managers.Inven.inven.Set_Inven_Info(Managers.Inven.changeSpot.index,count,_name);
        }
        else
        {
            Managers.Inven.inven.Set_Inven_Info(keyId, Managers.Inven.hotBar_itemInfo[Managers.Inven.changeSpot.index].count, Managers.Inven.hotBar_itemInfo[Managers.Inven.changeSpot.index].idName);
            Managers.Inven.hotBar.Set_HotBar_Info(Managers.Inven.changeSpot.index, count, _name);
        }
    }

    void MoveItemSpot()
    {
        string _name = Managers.Inven.inven_itemInfo[keyId].idName;
        int count = Managers.Inven.inven_itemInfo[keyId].count;
        if (Managers.Inven.changeSpot.invenType == Define.InvenType.Inven)
        {
            Managers.Inven.inven.Set_Inven_Info(keyId, 0);
            Managers.Inven.inven.Set_Inven_Info(Managers.Inven.changeSpot.index, count, _name);
        }
        else
        {
            Managers.Inven.inven.Set_Inven_Info(keyId, 0);
            Managers.Inven.hotBar.Set_HotBar_Info(Managers.Inven.changeSpot.index, count, _name);
        }
    }

    void CombineItem()
    {
        string _name = Managers.Inven.inven_itemInfo[keyId].idName;
        int count = Managers.Inven.inven_itemInfo[keyId].count;
        if (Managers.Inven.changeSpot.invenType == Define.InvenType.Inven)
        {
            Managers.Inven.inven.Set_Inven_Info(keyId, 0);
            Managers.Inven.inven.Set_Inven_Info(Managers.Inven.changeSpot.index, count + Managers.Inven.inven_itemInfo[Managers.Inven.changeSpot.index].count,_name);
        }
        else
        {
            Managers.Inven.inven.Set_Inven_Info(keyId, 0);
            Managers.Inven.hotBar.Set_HotBar_Info(Managers.Inven.changeSpot.index, count + Managers.Inven.hotBar_itemInfo[Managers.Inven.changeSpot.index].count,_name);
        }
    }
}
