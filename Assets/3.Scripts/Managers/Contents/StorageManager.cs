using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;
using UnityEngine.Tilemaps;
using static StorageManager;
using static UnityEditor.Progress;

public class StorageManager : MonoBehaviour
{
    public int Coin {  get { return _coin; } set 
        {
            _coin = value;
            if(inventoryUI != null)
                inventoryUI.SetCoin();
        }
    }
    int _coin =0;


    //ItemInfo대신 Item으로 바꾸기
    public SlotInfo[] hotBarSlotInfo = new SlotInfo[5];
    public SlotInfo[] inventorySlotInfo = new SlotInfo[24];
    public SlotInfo smeltSlotInfo = new SlotInfo(0);
    public SlotInfo grillingSlotInfo = new SlotInfo(0);

    public class SlotInfo
    {
        public ItemSO itemInfo;
        public int count;
        public KeyType keyType;
        public TileBase tile;

        public SlotInfo(int count,string _name = "")
        {
            if(_name == "")
            {
                itemInfo = null;
                count = 0;
                keyType = KeyType.Empty;
                return;
            }

            itemInfo = Resources.Load<GameObject>($"Prefabs/Items/{_name}").GetComponent<Item>().itemSo; //이름으로 가져오기
            keyType = KeyType.Exist;
            this.count = count;
            if (itemInfo.itemType == ItemType.Building)
                tile = Resources.Load<TileBase>($"TileMap/{_name}");
        }
    }

    public void Init()
    {
        if (hotBarUI == null)
        {
            hotBarUI = FindAnyObjectByType<UI_HotBar>();
            if (hotBarUI == null)
            {
                GameObject go = Managers.UI.ShowUI("UI_Storage");
                hotBarUI = Util.FindChild(go, "UI_HotBar").GetComponent<UI_HotBar>();
            }
            hotBarUI.Init();
        }
        if (inventoryUI == null)
        {
            inventoryUI = FindAnyObjectByType<UI_Inventory>();
            if (inventoryUI == null)
            {
                GameObject go = Managers.UI.ShowUI("UI_Storage");
                inventoryUI = Util.FindChild(go, "UI_Inven").GetComponent<UI_Inventory>();
            }
            inventoryUI.Init();
        }
        if (smeltUI == null)
        {
            smeltUI = FindAnyObjectByType<UI_Smelt>();
            if (smeltUI == null)
            {
                GameObject go = Managers.UI.ShowUI("UI_Storage");
                smeltUI = Util.FindChild(go, "UI_Inven").GetComponent<UI_Smelt>();
            }
        }
    }

    #region 인벤토리
    public UI_HotBar hotBarUI;
    public UI_Inventory inventoryUI;
    public UI_Smelt smeltUI;

    public int choiceIndex = 0;
    public bool choicingTower = false;

    //선택한 값에 따라 다르게 실행
    public void CheckHotBarChoice()
    {
        ItemSO info = hotBarUI.slotList[choiceIndex].UI_item.slotInfo.itemInfo;
        if (info == null)
            return;
        //체크할 때 플레이어의 검이 있을 때마다 지운다(수정 필요) -> 무기는 공격이 끝나면 사라져서 안보이고
        if (Managers.Game.weapon != null)
            Destroy(Managers.Game.weapon);
        switch (info.itemType)
        {
            case ItemType.Building:
                Managers.Game.mouse.CursorType = CursorType.Builder;
                Managers.Game.build.GetBuildItemInfo(hotBarUI.slotList[choiceIndex].UI_item);
                Managers.Game.build.ShowBuildIcon();
                break;
            case ItemType.Tower:
                Managers.Game.mouse.CursorType = CursorType.Builder;
                Managers.Game.build.HideBuildIcon();
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
        if (!Managers.Game.isKeepingTower)
        {
            Managers.Game.mouse.CursorType = CursorType.Normal;
            hotBarUI.towerSlot.HideTowerIcon();
        }
        else
            Managers.Game.mouse.CursorType = CursorType.Builder;
    }

    public bool AddOneItem(string name)
    {
        ItemSO item = Resources.Load<Item>($"Prefabs/Items/{name}").itemSo;
        if (item.itemType != ItemType.Tool)   //재료 아이템일때
        {
            UI_Item emptySlot = null;
            for (int i = 0; i < inventoryUI.slotList.Count - 1; i++)
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
                    SetSlot(item,itemUI, itemUI.slotInfo.count + 1);
                    return true;
                }
            }
            //추가 하지 못했다면 비어있는 칸에 넣기
            if (emptySlot != null)
            {
                SetSlot(item,emptySlot,1);
                return true;
            }

            //추가 하지 못했는데 비어있는 칸도 없을때
            return false;
        }
        else //도구 아이템일때
        {
            for (int i = 0; i < inventoryUI.slotList.Count - 1; i++)
            {
                UI_Item itemUI = inventoryUI.slotList[i].itemUI;
                if (KeyType.Empty == itemUI.slotInfo.keyType)
                {
                    SetSlot(item, itemUI,1);
                    return true;
                }
            }

            return false;
        }
    }

    public void AddItems(string name,int count)
    {
        ItemSO item = Resources.Load<Item>($"Prefabs/Items/{name}").itemSo;
        UI_Item emptySlot = null;
        for (int i = 0; i < inventoryUI.slotList.Count - 1; i++)
        {
            UI_Item itemUI = inventoryUI.slotList[i].itemUI;
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
                }
                else
                {
                    SetSlot(item, itemUI, itemUI.slotInfo.count + count);
                    break;
                }
            }
        }
        //추가 하지 못했다면 비어있는 칸에 넣기
        if (emptySlot != null)
        {
            SetSlot(item, emptySlot, count);
        }
    }
    #endregion





    //i1 : 드래그하는 아이템, i2 : 드래그를 드랍한 아이템
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
        if(drop.slotInfo.count > drop.slotInfo.itemInfo.maxAmount)
        {
            drag.slotInfo.count = drop.slotInfo.count - drop.slotInfo.itemInfo.maxAmount;
            drop.slotInfo.count = drop.slotInfo.itemInfo.maxAmount;
        }
        else drag.MakeEmptySlot();
        drag.SetInfo();
        drop.SetInfo();
    }

    public void SetSlot(ItemSO item,UI_Item itemUI,int count)
    {
        itemUI.slotInfo.itemInfo = item;
        itemUI.slotInfo.keyType = KeyType.Exist;
        itemUI.slotInfo.count = count;
        itemUI.SetInfo();
    }
}

