using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;
using UnityEngine.Tilemaps;
using System.Threading;
using static StorageManager;

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

    public class SlotInfo
    {
        public Item itemInfo;
        public int count;
        public KeyType keyType;
        public TileBase tile;

        public SlotInfo(int _count,string _name = "")
        {
            if(_name == "")
            {
                itemInfo = null;
                count = 0;
                keyType = KeyType.Empty;
                return;
            }

            itemInfo = Resources.Load<GameObject>($"Prefabs/Items/{_name}").GetComponent<Item>(); //이름으로 가져오기
            keyType = KeyType.Exist;
            this.count = _count;
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
    }

    #region 인벤토리
    public UI_HotBar hotBarUI;
    public int choiceIndex = 0;


    //선택한 값에 따라 다르게 실행
    public void CheckHotBarChoice()
    {
        Item info = hotBarUI.slotList[choiceIndex].UI_item.slotInfo.itemInfo;

        //체크할 때 플레이어의 검이 있을 때마다 지운다(수정 필요) -> 무기는 공격이 끝나면 사라져서 안보이고
        switch (info.itemType)
        {
            case ItemType.Building:
                Managers.Game.mouse.CursorType = CursorType.Builder;
                break;
            case ItemType.Tower:
                Managers.Game.mouse.CursorType = CursorType.Builder;
                break;
            case ItemType.Tool:
                Managers.Game.mouse.CursorType = CursorType.Normal;
                if (Managers.Game.weapon != null)
                    Destroy(Managers.Game.weapon);
                Managers.Game.weapon = Instantiate(Resources.Load<GameObject>($"Prefabs/Items/{info.idName}"), Managers.Game.player.toolParent.transform);
                break;
            default:
                Managers.Game.mouse.CursorType = CursorType.Normal;

                break;
        }
    }

    public void CheckHotBarTowerSlot()
    {

    }

    public bool AddOneItem(string _name)
    {
        Item item = Resources.Load<Item>($"Prefabs/Items/{_name}");
        if (item.itemType != ItemType.Tool)   //재료 아이템일때
        {
            SlotInfo emptySlot = null;
            for (int i = 0; i < inventoryUI.slotList.Count - 1; i++)
            {
                SlotInfo slotInfo = inventoryUI.slotList[i].itemUI.slotInfo;
                if (slotInfo.itemInfo == null)
                {
                    if (emptySlot == null)
                        emptySlot = slotInfo;
                    continue;
                }

                if (item.idName == slotInfo.itemInfo.idName && slotInfo.count < 99)
                {
                    SetSlot(item,slotInfo, slotInfo.count + 1);
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
                SlotInfo slotInfo = inventoryUI.slotList[i].itemUI.slotInfo;
                if (KeyType.Empty == slotInfo.keyType)
                {
                    SetSlot(item, slotInfo,1);
                    return true;
                }
            }

            return false;
        }
    }
    #endregion


    public UI_Inventory inventoryUI;

    //i1 : 드래그하는 아이템, i2 : 드래그를 드랍한 아이템
    public void ChangeItem(UI_Item i1, UI_Item i2)
    {
        SlotInfo change1 = i1.slotInfo;
        SlotInfo change2 = i2.slotInfo;
        i1.slotInfo = change2;
        i2.slotInfo = change1;
        i1.SetInfo();
        i2.SetInfo();
    }

    public void AddItem(UI_Item i1,UI_Item i2)
    {
        i2.slotInfo.count += i1.slotInfo.count;
        if(i2.slotInfo.count > 99)
        {
            i1.slotInfo.count = i2.slotInfo.count - 99;
            i2.slotInfo.count = 99;
        }
        else i1.MakeEmptySlot();
        i1.SetInfo();
        i2.SetInfo();
    }

    public void SetSlot(Item item,SlotInfo slotInfo,int count)
    {
        slotInfo.itemInfo = item;
        slotInfo.keyType = KeyType.Exist;
        slotInfo.count = count;
    }
}

