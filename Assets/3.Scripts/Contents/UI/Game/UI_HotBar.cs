using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class UI_HotBar : UI_Base
{
    public static Action tutorialEvent;

    public AssetReferenceGameObject hotBarSlotAsset;
    public AssetReferenceGameObject towerSlotAsset;

    [HideInInspector]
    public UI_HotbarSlot[] slotList;
    [HideInInspector]
    public UI_TowerSlot towerSlot;
    Image choice;
    GameObject grid;

    int choiceIndex = -1;

    public override void Init()
    {
        if (_init) return;
        _init = true;
        choice = Util.FindChild(gameObject, "Choice", true).GetComponent<Image>();
        grid = Util.FindChild(gameObject, "Grid", true);

        GetData();
        MakeKeys();
    }

    void OnKey1()
    {
        ChangeChoice(0);
    }

    void OnKey2()
    {
        ChangeChoice(1);
    }

    void OnKey3()
    {
        ChangeChoice(2);
    }

    void OnKey4()
    {
        ChangeChoice(3);
    }

    void OnKey5()
    {
        ChangeChoice(4);
    }

    void OnUpScroll()
    {
        int index = choiceIndex + 1;
        if (index > 4)
            index = 0;
        ChangeChoice(index);
    }

    void OnDownScroll()
    {
        int index = choiceIndex - 1;
        if (index < 0)
            index = 4;
        ChangeChoice(index);
    }

    void MakeKeys()
    {
        slotList = new UI_HotbarSlot[Managers.Inven.hotBarSlotInfo.Length];
        hotBarSlotAsset.LoadAssetAsync().Completed += (slot) =>
        {
            towerSlotAsset.LoadAssetAsync().Completed += (obj) =>
            {
                for (int i = 0; i < Managers.Inven.hotBarSlotInfo.Length - 1; i++)
                {
                    UI_HotbarSlot hotBarSlot = Instantiate(slot.Result, grid.transform).GetComponent<UI_HotbarSlot>();

                    slotList[i] = hotBarSlot;
                    hotBarSlot.GetComponentInChildren<UI_Item>().slotInfo = Managers.Inven.hotBarSlotInfo[i];
                    hotBarSlot.GetComponentInChildren<UI_Item>().Init();
                    slotList[i].GetComponent<UI_HotbarSlot>().Init();
                }
                towerSlot = Instantiate(obj.Result, grid.transform).GetComponent<UI_TowerSlot>();
                towerSlot.Init();
                ChangeChoice(0);

                Managers.Game.isKeepingTower = true;
                Managers.Inven.hotBarUI.CheckChoice();
                Managers.Inven.hotBarUI.towerSlot.ShowTowerIcon();
                Managers.Game.tower.transform.SetParent(Managers.Game.build.transform);
                Managers.Game.tower.transform.position = Managers.Game.build.transform.position;
            };
        };
        

    }


    //선택한 핫바키를 바꿔주는 함수
    void ChangeChoice(int change)
    {
        if (Managers.Game.mouse.CursorType == Define.CursorType.Drag)
            return;
        if (choiceIndex == change)
            return;

        if (!Managers.Game.completeTutorial)
            tutorialEvent.Invoke(); 
        choiceIndex = change;
        choice.GetComponent<RectTransform>().anchoredPosition = new Vector2(-305 + change * 135, -475);

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
        Managers.Inven.hotBarSlotInfo[0] = new StorageManager.SlotInfo(16, "Fence");
        Managers.Inven.hotBarSlotInfo[1] = new StorageManager.SlotInfo(1, "Sword");
        Managers.Inven.hotBarSlotInfo[2] = new StorageManager.SlotInfo(3, "Cannon");
        for (int i = 3; i < Managers.Inven.hotBarSlotInfo.Length; i++)
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
