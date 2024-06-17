using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class UI_HotBar : UI_Base
{
    List<UI_HotBar_Key> keys = new List<UI_HotBar_Key>();

    public override void Init()
    {
        if (keys.Count != 0)
          return;
        GetData();
        MakeKeys();
        keys[keys.Count - 1].GetComponent<Image>().color = Color.yellow;
        keys[Managers.Inven.hotBar_choice].GetComponent<UI_HotBar_Key>().choice.SetActive(true);
        keys[keys.Count - 1].SetTowerIcon();
        GetComponent<Canvas>().worldCamera = Camera.main;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            ChangeChoice(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ChangeChoice(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ChangeChoice(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            ChangeChoice(3);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            ChangeChoice(4);
        }
    }

    void MakeKeys()
    {
        UI_HotBar_Key go;
        for (int i = 0; i < Managers.Inven.hotBar_itemInfo.Length-1; i++)
        {
            go = Instantiate(Resources.Load<GameObject>("UI/UI_Hotbar/Key"), transform.GetChild(0)).GetComponent<UI_HotBar_Key>();
            keys.Add(go);
            keys[i].GetComponent<UI_HotBar_Key>().Init();
            go.keyId = i;
            SetKeys(i);
        }
        go = Instantiate(Resources.Load<GameObject>("UI/UI_Hotbar/Key"), transform.GetChild(0)).GetComponent<UI_HotBar_Key>();
        keys.Add(go);
        keys[Managers.Inven.hotBar_itemInfo.Length - 1].GetComponent<UI_HotBar_Key>().Init();
        go.keyId = Managers.Inven.hotBar_itemInfo.Length - 1;
    }

    //선택한 핫바키를 바꿔주는 함수
    void ChangeChoice(int change)
    {
        keys[Managers.Inven.HotBar_Choice].GetComponent<UI_HotBar_Key>().UnChoice();
        Managers.Inven.HotBar_Choice = change;
        keys[Managers.Inven.HotBar_Choice].GetComponent<UI_HotBar_Key>().Choice();
    }

    #region 아이템 관련
    //값 가져오기
    public void GetData()
    {
        int a =5;
        Managers.Inven.hotBar_itemInfo[0] = new InvenManager.ItemInfo(10,Define.InvenType.HotBar,"Fence_Lv.1");
        Managers.Inven.hotBar_itemInfo[1] = new InvenManager.ItemInfo(1, Define.InvenType.HotBar,"Sword");
        Managers.Inven.hotBar_itemInfo[2] = new InvenManager.ItemInfo(3, Define.InvenType.HotBar, "Turret_Lv.1");
        for(int i = 3; i < a; i++)
        {
            Managers.Inven.hotBar_itemInfo[i] = new InvenManager.ItemInfo(0, Define.InvenType.HotBar);
        }
    }

    //핫바에 정보 보여주기
   public void SetKeys(int index)
    {
        keys[index].GetComponent<UI_HotBar_Key>().SetIcon();
    }

    //아이템 정보를 넣어줌
    public void Set_HotBar_Info(int key_index, int count,Item item = null)
    {
        Managers.Inven.hotBar_itemInfo[key_index].itemInfo = item;
        if(item == null)
        {
            keys[key_index].GetComponent<UI_HotBar_Key>().EmptyKey();
            return;
        }

        if (count > 99)
        {
            Managers.Inven.AddItem(item.idName,count - 99);
            count = 99;
        }
        
        Managers.Inven.hotBar_itemInfo[key_index].count = count;
        if (item.itemType == Define.ItemType.Building)   //건설 아이템은 타일을 따로 가지고 있는다
            Managers.Inven.hotBar_itemInfo[key_index].tile = Resources.Load<TileBase>($"TileMap/{item.itemName}");
        Managers.Inven.hotBar_itemInfo[key_index].keyType = Define.KeyType.Exist;

        SetKeys(key_index);
    }
    #endregion

    #region 기지 관련

    public void GetTower()
    {
        Managers.Inven.hotBar_itemInfo[keys.Count - 1].itemInfo.itemType = Define.ItemType.Tower;
        Managers.Inven.hotBar_itemInfo[keys.Count - 1].keyType = Define.KeyType.Exist;
        keys[keys.Count - 1].SetTowerIcon();
        Managers.Inven.Set_HotBar_Choice();
    } 
    #endregion
}
