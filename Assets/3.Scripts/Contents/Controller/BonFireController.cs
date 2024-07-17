using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BonFireController : MonoBehaviour, ICaninteract
{
    public bool canInteract { get; set; }
    public GameObject canInteractSign { get; set; }

    UI_Item choicingItemUI;

    [HideInInspector]
    public List<ItemBaker> itemBakers = new List<ItemBaker>();
    public int maxCanBakeCount;

    ItemBaker itemBakerObj;

    void Start()
    {
        canInteractSign = Util.FindChild(gameObject, "Sign");
        canInteractSign.SetActive(false);
        itemBakerObj = Resources.Load<ItemBaker>("Prefabs/ItemBaker");
        for(int i = 0; i < maxCanBakeCount; i++)
        {
            itemBakers.Add(Instantiate(itemBakerObj,transform).GetComponent<ItemBaker>());
        }
    }

    public void Interact()
    {
        if (Managers.Game.isKeepingTower)
            return;

        choicingItemUI = Managers.Inven.hotBarUI.slotList[Managers.Inven.choiceIndex].itemUI;
        if (choicingItemUI.slotInfo.itemInfo != null)
        {
            if (choicingItemUI.slotInfo.itemInfo.bakeItemSO != null && itemBakers.Count > 0)
            {
                choicingItemUI.slotInfo.count -= 1;
                choicingItemUI.SetInfo();
                StartCoroutine(itemBakers[0].Bake(choicingItemUI.slotInfo.itemInfo));
                itemBakers.Remove(itemBakers[0]);
            }
        }
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
}
