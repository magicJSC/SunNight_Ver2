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

    RectTransform choiceRect;
    GameObject grid;

    int choiceIndex = -1;

    public override void Init()
    {
        if (_init) return;
        _init = true;
        choice = Util.FindChild(gameObject, "Choice", true).GetComponent<Image>();
        grid = Util.FindChild(gameObject, "Grid", true);
        choiceRect = choice.GetComponent<RectTransform>();

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
        slotList = new UI_HotbarSlot[5];
        hotBarSlotAsset.LoadAssetAsync().Completed += (slot) =>
        {
            towerSlotAsset.LoadAssetAsync().Completed += (obj) =>
            {
                for (int i = 0; i < 5; i++)
                {
                    UI_HotbarSlot hotBarSlot = Instantiate(slot.Result, grid.transform).GetComponent<UI_HotbarSlot>();
                    slotList[i] = hotBarSlot;
                    slotList[i].Init();
                    slotList[i].itemUI.slotInfo = Managers.Inven.hotBarSlotInfo[i];
                    hotBarSlot.GetComponentInChildren<UI_Item>().Init();
                }
                ChangeChoice(0);

                Managers.Inven.hotBarUI.CheckChoice();
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
        choiceRect.anchoredPosition = new Vector2(-305 + change * 135, -475);

        Managers.Inven.choiceIndex = change;
        CheckChoice();
    }

    public void CheckChoice()
    {
        Managers.Inven.CheckHotBarChoice();
    }

   

    #region 기지 관련

    public void GetTower()
    {
        Managers.Inven.hotBarUI.slotList[4].itemUI.slotInfo.keyType = Define.KeyType.Exist;
    }
    #endregion
}
