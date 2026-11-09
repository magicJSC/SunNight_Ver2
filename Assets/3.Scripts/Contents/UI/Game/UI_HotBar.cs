using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class UI_HotBar : UI_Base
{
    public AssetReferenceGameObject hotBarSlotAsset;
    Image carriedBaseIcon;
    float nextLegacySlotCheck;

    [HideInInspector]
    public UI_HotbarSlot[] slotList;
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

        GetComponent<Canvas>().worldCamera = Camera.main;

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

    public void SelectBaseSlot()
    {
        ChangeChoice(InvenManager.BaseSlotIndex);
    }

    void OnKey5()
    {
        SelectBaseSlot();
    }

    void OnDownScroll()
    {
        int index = choiceIndex + 1;
        if (index > 4)
            index = 0;
        ChangeChoice(index);
    }

    void OnUpScroll()
    {
        int index = choiceIndex - 1;
        if (index < 0)
            index = 4;
        ChangeChoice(index);
    }

    void MakeKeys()
    {
        slotList = new UI_HotbarSlot[InvenManager.BaseSlotIndex + 1];
        hotBarSlotAsset.LoadAssetAsync().Completed += slot =>
        {
            for (int i = 0; i < slotList.Length; i++)
            {
                UI_HotbarSlot hotBarSlot = Instantiate(slot.Result, grid.transform).GetComponent<UI_HotbarSlot>();
                slotList[i] = hotBarSlot;
                hotBarSlot.Init();
                hotBarSlot.IsBaseSlot = i == InvenManager.BaseSlotIndex;
                hotBarSlot.itemUI.slotInfo = hotBarSlot.IsBaseSlot ? new InvenManager.SlotInfo(0) : Managers.Inven.hotBarSlotInfo[i];
                hotBarSlot.itemUI.Init();
                if (hotBarSlot.IsBaseSlot)
                    CreateBaseIcon(hotBarSlot.transform);
            }
            choiceIndex = 0;
            choiceRect.anchoredPosition = new Vector2(-305, -475);
            Managers.Inven.choiceIndex = 0;
            CheckChoice();
        };
    }


    //선택한 핫바키를 바꿔주는 함수
    void ChangeChoice(int change)
    {
        if (Managers.Game.mouse.CursorType == Define.CursorType.Drag)
            return;
        if (choiceIndex == change)
            return;
        if (Managers.Game.isCantPlay)
            return;
        choiceIndex = change;
        choiceRect.anchoredPosition = new Vector2(-305 + change * 135, -475);



        Managers.Inven.choiceIndex = change;
        CheckChoice();
    }

    void CreateBaseIcon(Transform parent)
    {
        var icon = new GameObject("CarriedBase", typeof(RectTransform), typeof(Image));
        icon.transform.SetParent(parent, false);
        carriedBaseIcon = icon.GetComponent<Image>();
        carriedBaseIcon.raycastTarget = false;
        carriedBaseIcon.preserveAspect = true;
        var rect = carriedBaseIcon.rectTransform;
        rect.anchorMin = new Vector2(.15f, .15f);
        rect.anchorMax = new Vector2(.85f, .85f);
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        carriedBaseIcon.enabled = false;
    }

    void LateUpdate()
    {
        if (!carriedBaseIcon || !Managers.HasInstance) return;
        bool carried = Managers.Game.isMovingTower && Managers.Game.tower != null;
        if (carried && !carriedBaseIcon.enabled)
            carriedBaseIcon.sprite = Managers.Game.tower.GetComponent<SpriteRenderer>().sprite;
        carriedBaseIcon.enabled = carried;

        // Old saves can have a fifth ordinary stack. Keep any overflow in the save
        // until a regular inventory slot has space again.
        var legacy = Managers.Inven.hotBarSlotInfo[InvenManager.BaseSlotIndex];
        if (legacy == null || legacy.itemInfo == null || Time.unscaledTime < nextLegacySlotCheck) return;
        nextLegacySlotCheck = Time.unscaledTime + 1f;
        FieldInventory.RestoreLegacyStack(legacy);
    }

    public void CheckChoice()
    {
        Managers.Inven.CheckHotBarChoice();
    }
}
