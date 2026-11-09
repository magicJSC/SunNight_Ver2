using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BonFireController : MonoBehaviour, IInteractObject
{
    public bool canInteract { get; set; }
    public GameObject canInteractSign { get; set; }



    Text countText;
    UI_Item choicingItemUI;

    [HideInInspector]
    public List<ItemBaker> itemBakers = new List<ItemBaker>();
    public int BakingCount { get { return bakingCount; }set { bakingCount = value; SetCountText(); } }
    int bakingCount = 0;
    public int maxCanBakeCount;

    ItemBaker itemBakerObj;

    void Start()
    {
        canInteractSign = Util.FindChild(gameObject, "Sign");
        countText = Util.FindChild(gameObject, "Count",true).GetComponent<Text>();
        canInteractSign.SetActive(false);
        itemBakerObj = Resources.Load<ItemBaker>("Prefabs/ItemBaker");
        countText.text = "";
    }

    public void Interact()
    {
        if (Managers.Game.isMovingTower)
            return;

        choicingItemUI = Managers.Inven.hotBarUI.slotList[Managers.Inven.choiceIndex].itemUI;
        if (choicingItemUI.slotInfo.itemInfo != null)
        {
            if (choicingItemUI.slotInfo.itemInfo.bakeItem != null && BakingCount < maxCanBakeCount)
            {
                var baker = itemBakers.Count == 0 ? Instantiate(itemBakerObj, transform) : itemBakers[0];
                itemBakers.Remove(baker);
                baker.Bake(choicingItemUI.slotInfo.itemInfo);
                choicingItemUI.slotInfo.count -= 1;
                choicingItemUI.SetInfo();
                BakingCount++;
            }
        }
    }

    void SetCountText()
    {
        if (bakingCount != 0)
            countText.text = $"{bakingCount}/{maxCanBakeCount}";
        else
            countText.text = "";
    }

    public void ShowInteractSign()
    {
        canInteractSign.SetActive(true);
    }

    public void HideInteractSign()
    {
        canInteractSign.SetActive(false);
    }

    public void SetAction(PlayerInteract playerInteract)
    {
        playerInteract.interactAction += Interact;
    }

    public void CancelAction(PlayerInteract playerInteract)
    {
        playerInteract.interactAction -= Interact;
    }
}
