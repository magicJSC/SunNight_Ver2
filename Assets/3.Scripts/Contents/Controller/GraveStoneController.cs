using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraveStoneController : MonoBehaviour,IInteractObject
{
    public GameObject canInteractSign { get; private set; }

    [HideInInspector]
    public List<InvenManager.SlotInfo> slotInfoList = new List<InvenManager.SlotInfo>();

    void Start()
    {
        canInteractSign = Util.FindChild(gameObject, "Sign");
        canInteractSign.SetActive(false);
        GetInvenData();
    }


    void GetInvenData()
    {
        UI_InventorySlot[] invenSlotInfo = Managers.Inven.inventoryUI.slotList;
        foreach(UI_InventorySlot info in invenSlotInfo)
        {
            if(info.itemUI.slotInfo.itemInfo != null)
            {
                InvenManager.SlotInfo slotInfo = new InvenManager.SlotInfo(info.itemUI.slotInfo.count, info.itemUI.slotInfo.itemInfo.idName);
                slotInfoList.Add(slotInfo);
            }
        }
        UI_HotbarSlot[] hotbarSlotInfo = Managers.Inven.hotBarUI.slotList;
        for(int i = 0; i < hotbarSlotInfo.Length - 1; i++)
        {
            if (hotbarSlotInfo[i].itemUI.slotInfo.itemInfo != null)
            {
                InvenManager.SlotInfo slotInfo = new InvenManager.SlotInfo(hotbarSlotInfo[i].itemUI.slotInfo.count, hotbarSlotInfo[i].itemUI.slotInfo.itemInfo.idName);
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
        if(slotInfoList.Count == 0)
            Destroy(gameObject);
    }

    void GetItem()
    {
        int stack =0;
        int count = slotInfoList.Count;
        for(int i = 0;i < count;i++)
        {
            if (Managers.Inven.GetItem(slotInfoList[stack].itemInfo, slotInfoList[stack].count) == 0)
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

    public void ShowInteractSign()
    {
        canInteractSign.SetActive(true);
    }

    public void HideInteractSign()
    {
        canInteractSign.SetActive(true);
    }

    public void SetAction(PlayerInteract playerInteract)
    {
        playerInteract.interactAction += ShowInteractSign;
    }

    public void CancelAction(PlayerInteract playerInteract)
    {
        playerInteract.interactAction -= ShowInteractSign;
    }
}
