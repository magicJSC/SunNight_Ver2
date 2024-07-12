using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraveStoneController : MonoBehaviour,ICaninteract
{
    public GameObject canInteractSign { get; private set; }

    [HideInInspector]
    public List<StorageManager.SlotInfo> ItemUIList = new List<StorageManager.SlotInfo>();

    void Start()
    {
        canInteractSign = Util.FindChild(gameObject, "Sign");
        canInteractSign.SetActive(false);
        GetInvenData();
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<PlayerController>(out var player))
        {
            EnterPlayer(player);
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<PlayerController>(out var player))
        {
            ExitPlayer(player);
        }
    }

    public void EnterPlayer(PlayerController player)
    {
        player.interactObjectList.Add(gameObject);
        player.SetInteractObj();
    }

    public void ExitPlayer(PlayerController player)
    {
        player.interactObjectList.Remove(gameObject);
        canInteractSign.SetActive(false);
        player.SetInteractObj();
    }

    void GetInvenData()
    {
        UI_InventorySlot[] invenSlotInfo = Managers.Inven.inventoryUI.slotList;
        foreach(UI_InventorySlot info in invenSlotInfo)
        {
            if(info.itemUI.slotInfo.itemInfo != null)
            {
                StorageManager.SlotInfo slotInfo = new StorageManager.SlotInfo(info.itemUI.slotInfo.count, info.itemUI.slotInfo.itemInfo.idName);
                ItemUIList.Add(slotInfo);
            }
        }
        UI_HotbarSlot[] hotbarSlotInfo = Managers.Inven.hotBarUI.slotList;
        for(int i = 0; i < hotbarSlotInfo.Length - 1; i++)
        {
            if (hotbarSlotInfo[i].itemUI.slotInfo.itemInfo != null)
            {
                StorageManager.SlotInfo slotInfo = new StorageManager.SlotInfo(hotbarSlotInfo[i].itemUI.slotInfo.count, hotbarSlotInfo[i].itemUI.slotInfo.itemInfo.idName);
                ItemUIList.Add(slotInfo); 
            }
        }
        int index;
        int count = ItemUIList.Count / 4;
        for(int i = 0; i < count; i++)
        {
            index = Random.Range(0,ItemUIList.Count);
            ItemUIList.Remove(ItemUIList[index]);
        }
        Managers.Inven.EmptyInvenAndHotBar();
    }

    public void Interact()
    {
        GetItem();
    }

    void GetItem()
    {
        bool bringAll = true;
       foreach(StorageManager.SlotInfo item in ItemUIList)
       {
            if(!Managers.Inven.AddItems(item.itemInfo.idName, item.count))
                bringAll = false;
       }
        if (bringAll)
        {
            Destroy(gameObject);
            ItemUIList.Clear();
        }
        else
            Debug.Log("인벤이 차서 가져갈수 없습니다");
    }
}
