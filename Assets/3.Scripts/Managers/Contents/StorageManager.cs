using System;
using UnityEngine;
using static Define;

public class StorageManager : MonoBehaviour
{
    public int Coin
    {
        get { return _coin; }
        set
        {
            _coin = value;
            if (inventoryUI != null)
                inventoryUI.SetCoin();
        }
    }
    int _coin = 0;

    public SlotInfo[] hotBarSlotInfo = new SlotInfo[5];
    public SlotInfo[] inventorySlotInfo = new SlotInfo[24];
    public SlotInfo smeltSlotInfo = new SlotInfo(0);
    public SlotInfo grillingSlotInfo = new SlotInfo(0);
    public SlotInfo coalSlotInfo = new SlotInfo(0);

    public class SlotInfo
    {
        public ItemSO itemInfo;
        public int count;
        public KeyType keyType;

        public SlotInfo(int count, string _name = "")
        {
            if (_name == "")
            {
                itemInfo = null;
                count = 0;
                keyType = KeyType.Empty;
                return;
            }

            itemInfo = Resources.Load<GameObject>($"Prefabs/Items/{_name}").GetComponent<Item>().itemSo; //이름으로 가져오기
            keyType = KeyType.Exist;
            this.count = count;
        }
    }

    public void Init()
    {
        hotBarEvent = null;
        
        hotBarUI = Managers.UI.ShowInvenUI("UI_HotBar").GetComponent<UI_HotBar>();
        hotBarUI.Init();
      
        inventoryUI = Managers.UI.ShowInvenUI("UI_Inven").GetComponent<UI_Inventory>();
        inventoryUI.Init();
      
        smeltUI = Managers.UI.ShowInvenUI("UI_Smelting").GetComponent<UI_Smelt>();
        smeltUI.Init();
    }


    #region 인벤토리
    public UI_HotBar hotBarUI;
    public UI_Inventory inventoryUI;
    public UI_Smelt smeltUI;

    public int choiceIndex = 0;
    public bool choicingTower = false;

    public Action<UI_Item> hotBarEvent;

    //선택한 값에 따라 다르게 실행
    public void CheckHotBarChoice()
    {
        if (!hotBarUI.slotList[choiceIndex])
            return;
        ItemSO info = hotBarUI.slotList[choiceIndex].itemUI.slotInfo.itemInfo;
        //체크할 때 플레이어의 검이 있을 때마다 지운다(수정 필요) -> 무기는 공격이 끝나면 사라져서 안보이고
        if (Managers.Game.weapon != null)
            Destroy(Managers.Game.weapon);
        if (info == null)
        {
            Managers.Game.mouse.CursorType = CursorType.Normal;
            return;
        }
        hotBarEvent?.Invoke(hotBarUI.slotList[choiceIndex].itemUI);
        switch (info.itemType)
        {
            case ItemType.Building:
                Managers.Game.mouse.CursorType = CursorType.Builder;
                Managers.Game.build.gridSign.size = new Vector2(1, 1);
                Managers.Game.build.GetBuildItemInfo(hotBarUI.slotList[choiceIndex].itemUI);
                break;
            case ItemType.Tool:
                Managers.Game.mouse.CursorType = CursorType.Battle;
                Managers.Game.weapon = Instantiate(Resources.Load<GameObject>($"Prefabs/Items/{info.idName}"), Managers.Game.player.toolParent.transform);
                break;
            default:
                Managers.Game.mouse.CursorType = CursorType.Normal;
                break;
        }
    }

    public void CheckHotBarTowerSlot()
    {
        if (Managers.Game.weapon != null)
            Destroy(Managers.Game.weapon);

        if (!Managers.Game.isKeepingTower)
        {
            Managers.Game.mouse.CursorType = CursorType.Normal;
            hotBarUI.towerSlot.HideTowerIcon();
            Managers.Game.tower.AfterInstallTower();
        }
        else
        {
            Managers.Game.build.buildItemIcon.gameObject.SetActive(false);
            Managers.Game.build.gridSign.size = new Vector2(2, 2);
            Managers.Game.mouse.CursorType = CursorType.Builder;
            Managers.Game.tower.BeforeInstallTower();
        }
    }

    public bool AddOneItem(ItemSO item)
    {
        if (item.itemType != ItemType.Tool)   //재료 아이템일때
        {
            UI_Item emptySlot = null;
            for (int i = 0; i < inventoryUI.slotList.Length - 1; i++)
            {
                UI_Item itemUI = inventoryUI.slotList[i].itemUI;
                if (itemUI.slotInfo.itemInfo == null)
                {
                    if (emptySlot == null)
                        emptySlot = itemUI;
                    continue;
                }

                if (item.idName == itemUI.slotInfo.itemInfo.idName && itemUI.slotInfo.count < itemUI.slotInfo.itemInfo.maxAmount)
                {
                    SetSlot(item, itemUI, itemUI.slotInfo.count + 1);
                    return true;
                }
            }
            for (int i = 0; i < hotBarUI.slotList.Length - 1; i++)
            {
                UI_Item itemUI = hotBarUI.slotList[i].itemUI;
                if (itemUI.slotInfo.itemInfo == null)
                {
                    if (emptySlot == null)
                        emptySlot = itemUI;
                    continue;
                }

                if (item.idName == itemUI.slotInfo.itemInfo.idName && itemUI.slotInfo.count < itemUI.slotInfo.itemInfo.maxAmount)
                {
                    SetSlot(item, itemUI, itemUI.slotInfo.count + 1);
                    return true;
                }
            }
            //추가 하지 못했다면 비어있는 칸에 넣기
            if (emptySlot != null)
            {
                SetSlot(item, emptySlot, 1);
                return true;
            }

            //추가 하지 못했는데 비어있는 칸도 없을때
            return false;
        }
        else //도구 아이템일때
        {
            for (int i = 0; i < inventoryUI.slotList.Length - 1; i++)
            {
                UI_Item itemUI = inventoryUI.slotList[i].itemUI;
                if (KeyType.Empty == itemUI.slotInfo.keyType)
                {
                    SetSlot(item, itemUI, 1);
                    return true;
                }
            }

            return false;
        }
    }

    public bool AddItems(ItemSO item, int count)
    {
        UI_Item emptySlot = null;
        for (int i = 0; i < inventoryUI.slotList.Length - 1; i++)
        {
            UI_Item itemUI = inventoryUI.slotList[i].itemUI;
            if (itemUI == null)
                continue;
            if (itemUI.slotInfo.itemInfo == null)
            {
                if (emptySlot == null)
                    emptySlot = itemUI;
                continue;
            }

            if (item.idName == itemUI.slotInfo.itemInfo.idName)
            {
                if (itemUI.slotInfo.count + count > itemUI.slotInfo.itemInfo.maxAmount)
                {
                    int lefting = itemUI.slotInfo.count + count - itemUI.slotInfo.itemInfo.maxAmount;
                    SetSlot(item, itemUI, itemUI.slotInfo.itemInfo.maxAmount);
                    AddItems(item, lefting);
                    return true;
                }
                else
                {
                    SetSlot(item, itemUI, itemUI.slotInfo.count + count);
                    return true;
                }
            }
        }
        for (int i = 0; i < hotBarUI.slotList.Length - 1; i++)
        {
            UI_Item itemUI = hotBarUI.slotList[i].itemUI;
            if (itemUI.slotInfo.itemInfo == null)
            {
                if (emptySlot == null)
                    emptySlot = itemUI;
                continue;
            }

            if (item.idName == itemUI.slotInfo.itemInfo.idName)
            {
                if (itemUI.slotInfo.count + count > itemUI.slotInfo.itemInfo.maxAmount)
                {
                    int lefting = itemUI.slotInfo.count + count - itemUI.slotInfo.itemInfo.maxAmount;
                    SetSlot(item, itemUI, itemUI.slotInfo.itemInfo.maxAmount);
                    AddItems(item, lefting);
                    return true;
                }
                else
                {
                    SetSlot(item, itemUI, itemUI.slotInfo.count + count);
                    return true;
                }
            }
        }
        //추가 하지 못했다면 비어있는 칸에 넣기
        if (emptySlot != null)
        {
            SetSlot(item, emptySlot, count);
            return true;
        }
        else
            return false;
    }
    #endregion

    public void EmptyInvenAndHotBar()
    {
        UI_InventorySlot[] invenSlots = inventoryUI.slotList;
        foreach (UI_InventorySlot slot in invenSlots)
        {
            slot.itemUI.MakeEmptySlot();
        }
        UI_HotbarSlot[] hotbarSlots = hotBarUI.slotList;
        for (int i = 0; i < hotbarSlots.Length - 1; i++)
        {
            hotbarSlots[i].itemUI.MakeEmptySlot();
        }
        CheckHotBarChoice();
    }

    public void ChangeItem(UI_Item drag, UI_Item drop)
    {
        SlotInfo change1 = drag.slotInfo;
        SlotInfo change2 = drop.slotInfo;
        drag.slotInfo = change2;
        drop.slotInfo = change1;
        drag.SetInfo();
        drop.SetInfo();
    }

    public void AddItem(UI_Item drag, UI_Item drop)
    {
        drop.slotInfo.count += drag.slotInfo.count;
        if (drop.slotInfo.count > drop.slotInfo.itemInfo.maxAmount)
        {
            drag.slotInfo.count = drop.slotInfo.count - drop.slotInfo.itemInfo.maxAmount;
            drop.slotInfo.count = drop.slotInfo.itemInfo.maxAmount;
        }
        else drag.MakeEmptySlot();
        drag.SetInfo();
        drop.SetInfo();
    }

    public void SetSlot(ItemSO item, UI_Item itemUI, int count)
    {
        itemUI.slotInfo.itemInfo = item;
        itemUI.slotInfo.keyType = KeyType.Exist;
        itemUI.slotInfo.count = count;
        itemUI.SetInfo();
    }

    public static bool canAbandon;

    public int GetItemCount(ItemSO itemSO)
    {
        int count = 0;
        for (int i = 0; i < inventoryUI.slotList.Length - 1; i++)
        {
            UI_Item itemUI = inventoryUI.slotList[i].itemUI;
            if (itemUI.slotInfo.itemInfo == itemSO)
            {
                count += itemUI.slotInfo.count;
            }
        }
        for (int i = 0; i < hotBarUI.slotList.Length - 1; i++)
        {
            UI_Item itemUI = hotBarUI.slotList[i].itemUI;
            if (itemUI.slotInfo.itemInfo == itemSO)
            {
                count += itemUI.slotInfo.count;
            }
        }
        return count;
    }

    public void RemoveItem(ItemSO itemSO,int count)
    {
        int remainCount = count;
        for (int i = 0; i < inventoryUI.slotList.Length - 1; i++)
        {
            UI_Item itemUI = inventoryUI.slotList[i].itemUI;
            if (itemUI.slotInfo.itemInfo == itemSO)
            {
                remainCount -= itemUI.slotInfo.count;
                if (remainCount < 0)
                {
                    itemUI.slotInfo.count = -remainCount;
                    itemUI.SetInfo();
                    return;
                }
                else
                {
                    itemUI.MakeEmptySlot();
                    if (remainCount == 0)
                        return;
                }
            }
        }
        for (int i = 0; i < hotBarUI.slotList.Length - 1; i++)
        {
            UI_Item itemUI = hotBarUI.slotList[i].itemUI;
            if (itemUI.slotInfo.itemInfo == itemSO)
            {
                remainCount -= itemUI.slotInfo.count;
                if (remainCount < 0)
                {
                    itemUI.slotInfo.count = -remainCount;
                    return;
                }
                else
                {
                    itemUI.MakeEmptySlot();
                    if (remainCount == 0)
                        return;
                }
            }
        }
    }
}

