using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraveStoneController : MonoBehaviour,ICaninteract
{
    public GameObject canInteractSign { get; private set; }

    [HideInInspector]
    public List<StorageManager.SlotInfo> slotInfoList = new List<StorageManager.SlotInfo>();

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
                slotInfoList.Add(slotInfo);
            }
        }
        UI_HotbarSlot[] hotbarSlotInfo = Managers.Inven.hotBarUI.slotList;
        for(int i = 0; i < hotbarSlotInfo.Length - 1; i++)
        {
            if (hotbarSlotInfo[i].itemUI.slotInfo.itemInfo != null)
            {
                StorageManager.SlotInfo slotInfo = new StorageManager.SlotInfo(hotbarSlotInfo[i].itemUI.slotInfo.count, hotbarSlotInfo[i].itemUI.slotInfo.itemInfo.idName);
                slotInfoList.Add(slotInfo); 
            }
        }
        int index;
        int count = slotInfoList.Count / 4;
        for(int i = 0; i < count; i++)
        {
            index = Random.Range(0,slotInfoList.Count);
            slotInfoList.Remove(slotInfoList[index]);
        }
        Managers.Inven.EmptyInvenAndHotBar();
    }

    public void Interact()
    {
        GetItem();
    }

    void GetItem()
    {
        int stack =0;
        int count = slotInfoList.Count;
        for(int i = 0;i < count;i++)
        {
            if (Managers.Inven.AddItems(slotInfoList[stack].itemInfo, slotInfoList[stack].count))
                slotInfoList.RemoveAt(stack);
            else
                stack++;
        }
        if (slotInfoList.Count == 0)
        {
            Destroy(gameObject);
            slotInfoList.Clear();
        }
        else
            Debug.Log("인벤이 차서 가져갈수 없습니다");
    }
}
