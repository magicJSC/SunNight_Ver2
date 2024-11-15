using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static Define;
using static UnityEditor.Progress;

public class InvenManager : MonoBehaviour
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

    GameObject itemSign;

    public void Init()
    {
        hotBarEvent = null;

        hotBarUI = Managers.UI.ShowInvenUI("UI_HotBar").GetComponent<UI_HotBar>();
        hotBarUI.Init();

        inventoryUI = Managers.UI.ShowInvenUI("UI_Inven").GetComponent<UI_Inventory>();
        inventoryUI.Init();

        Addressables.LoadAssetAsync<GameObject>("UI/ItemSign").Completed += (obj) =>
        {
            itemSign = obj.Result;
        };
    }


    #region 인벤토리
    public UI_HotBar hotBarUI;
    public UI_Inventory inventoryUI;

    public int choiceIndex = 0;
    public bool choicingTower = false;

    public Action<UI_Item> hotBarEvent;


    public UI_Item currentItemUI;

    //선택한 값에 따라 다르게 실행
    public void CheckHotBarChoice()
    {
        if (!hotBarUI.slotList[choiceIndex])
            return;
        currentItemUI = hotBarUI.slotList[choiceIndex].itemUI;
        //체크할 때 플레이어의 검이 있을 때마다 지운다(수정 필요) -> 무기는 공격이 끝나면 사라져서 안보이고
        if (Managers.Game.weapon != null)
            Destroy(Managers.Game.weapon);
        MapManager.canBuild.gameObject.SetActive(false);
        if (currentItemUI.slotInfo.itemInfo == null)
        {
            Managers.Game.mouse.CursorType = CursorType.Normal;
            return;
        }
        hotBarEvent?.Invoke(hotBarUI.slotList[choiceIndex].itemUI);
        switch (currentItemUI.slotInfo.itemInfo.itemType)
        {
            case ItemType.Building:
                Managers.Game.mouse.CursorType = CursorType.Builder;
                MapManager.canBuild.gameObject.SetActive(true);
                Managers.Game.build.GetBuildItemInfo(hotBarUI.slotList[choiceIndex].itemUI);
                break;
            case ItemType.Tool:
                Managers.Game.mouse.CursorType = CursorType.Battle;
                Managers.Game.weapon = Instantiate(currentItemUI.slotInfo.itemInfo.itemPrefab, Managers.Game.player.toolParent.transform);
                break;
            default:
                Managers.Game.mouse.CursorType = CursorType.Normal;
                break;
        }
    }

    public int GetItem(ItemSO item, int count)
    {
        if (item.itemType == ItemType.Tool)
             return AddWeapon(item);
        else
            return AddItem(item, count);
    }
    
    int AddItem(ItemSO item, int count)
    {
        int addCount = 0;
        UI_Item emptySlot = null;
        for (int i = 0; i < hotBarUI.slotList.Length; i++)
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
                    addCount += count - lefting;
                    count = lefting;
                    continue;
                }
                else
                {
                    SetSlot(item, itemUI, itemUI.slotInfo.count + count);
                    addCount += count;
                    ShowItemSign(item, addCount);
                    return 0;
                }
            }
        }
        for (int i = 0; i < inventoryUI.slotList.Length; i++)
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
                    addCount += count - lefting;
                    count = lefting;
                    continue;
                }
                else
                {
                    SetSlot(item, itemUI, itemUI.slotInfo.count + count);
                    addCount += count;
                    ShowItemSign(item, addCount);
                    return 0;
                }
            }
        }

        //추가 하지 못했다면 비어있는 칸에 넣기
        if (emptySlot != null)
        {
            SetSlot(item, emptySlot, count);
            ShowItemSign(item, addCount);
            return 0;
        }
        else
        {
            return count;
        }
    }

    int AddWeapon(ItemSO item)
    {
        for (int i = 0; i < hotBarUI.slotList.Length; i++)
        {
            UI_Item itemUI = hotBarUI.slotList[i].itemUI;
            if (itemUI.slotInfo.itemInfo == null)
            {
                SetSlot(item, itemUI, 1);
                ShowItemSign(item, 1);
                return 0;
            }

        }
        for (int i = 0; i < inventoryUI.slotList.Length; i++)
        {
            UI_Item itemUI = inventoryUI.slotList[i].itemUI;
            if (itemUI.slotInfo.itemInfo == null)
            {
                SetSlot(item, itemUI, 1);
                ShowItemSign(item, 1);
                return 0;
            }
        }

        return 1;
    }
    #endregion

    public void EmptyInvenAndHotBar()
    {
        UI_HotbarSlot[] hotbarSlots = hotBarUI.slotList;
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            hotbarSlots[i].itemUI.MakeEmptySlot();
        }
        UI_InventorySlot[] invenSlots = inventoryUI.slotList;
        foreach (UI_InventorySlot slot in invenSlots)
        {
            slot.itemUI.MakeEmptySlot();
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
        for (int i = 0; i < hotBarUI.slotList.Length; i++)
        {
            UI_Item itemUI = hotBarUI.slotList[i].itemUI;
            if (itemUI.slotInfo.itemInfo == itemSO)
            {
                count += itemUI.slotInfo.count;
            }
        }
        for (int i = 0; i < inventoryUI.slotList.Length; i++)
        {
            UI_Item itemUI = inventoryUI.slotList[i].itemUI;
            if (itemUI.slotInfo.itemInfo == itemSO)
            {
                count += itemUI.slotInfo.count;
            }
        }
        
        return count;
    }

    public void RemoveItem(ItemSO itemSO, int count)
    {
        int remainCount = count;
        for (int i = 0; i < hotBarUI.slotList.Length; i++)
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
        for (int i = 0; i < inventoryUI.slotList.Length; i++)
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
        
    }

    void ShowItemSign(ItemSO itemSO,int count)
    {
        Instantiate(itemSign, Managers.Game.player.transform.position, Quaternion.identity).GetComponent<ItemSign>().SetItem(itemSO,count);
    }
}

