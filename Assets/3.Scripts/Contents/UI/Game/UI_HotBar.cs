using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class UI_HotBar : UI_Base
{
    public UI_HotbarSlot[] slotList;
    public UI_TowerSlot towerSlot;
    Image choice;
    GameObject grid;
    
    int choiceIndex = -1;

    public override void Init()
    {
       if(_init) return;
       _init = true;
        choice = Util.FindChild(gameObject,"Choice",true).GetComponent<Image>();
        grid = Util.FindChild(gameObject, "Grid", true);

        GetData();
        MakeKeys();
        ChangeChoice(0);
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
        slotList = new UI_HotbarSlot[Managers.Inven.hotBarSlotInfo.Length];
        UI_HotbarSlot slot1;
        for (int i = 0; i < Managers.Inven.hotBarSlotInfo.Length-1; i++)
        {
            slot1 = Instantiate(Resources.Load<GameObject>("UI/UI_HotBar_Slot"), grid.transform).GetComponent<UI_HotbarSlot>();
            slotList[i] = slot1;
            slot1.GetComponentInChildren<UI_Item>().slotInfo = Managers.Inven.hotBarSlotInfo[i];
            slot1.GetComponentInChildren<UI_Item>().Init();
            slotList[i].GetComponent<UI_HotbarSlot>().Init();
            slot1.keyId = i;
        }
        towerSlot = Instantiate(Resources.Load<GameObject>("UI/UI_Slot_Tower"), grid.transform).GetComponent<UI_TowerSlot>();
        towerSlot.GetComponent<UI_TowerSlot>().Init();
    }


    //선택한 핫바키를 바꿔주는 함수
    void ChangeChoice(int change)
    {
        if (choiceIndex == change)
            return;
        choiceIndex = change;
        choice.GetComponent<RectTransform>().anchoredPosition = new Vector2(-330+change*160,-443);

        Managers.Inven.choiceIndex = change;
        CheckChoice();
    }

    public void CheckChoice()
    {
        bool choiceTower = choiceIndex == Managers.Inven.hotBarSlotInfo.Length - 1;
        if (!choiceTower)
            Managers.Inven.CheckHotBarChoice();
        else
            Managers.Inven.CheckHotBarTowerSlot();

        Managers.Inven.choicingTower = choiceTower;
        if (Managers.Game.isKeepingTower)
            Managers.Game.tower.gameObject.SetActive(choiceTower);
    }

    #region 아이템 관련
    //값 가져오기
    public void GetData()
    {
        Managers.Inven.hotBarSlotInfo[0] = new StorageManager.SlotInfo(10,"Fence");
        Managers.Inven.hotBarSlotInfo[1] = new StorageManager.SlotInfo(1,"Sword");
        Managers.Inven.hotBarSlotInfo[2] = new StorageManager.SlotInfo(3, "Turret");
        for(int i = 3; i < Managers.Inven.hotBarSlotInfo.Length; i++)
        {
            Managers.Inven.hotBarSlotInfo[i] = new StorageManager.SlotInfo(0);
        }
    }
    #endregion

    #region 기지 관련

    public void GetTower()
    {
        Managers.Inven.hotBarSlotInfo[slotList.Length - 1].keyType = Define.KeyType.Exist;
    } 
    #endregion
}
