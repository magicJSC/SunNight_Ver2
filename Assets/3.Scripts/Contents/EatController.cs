using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class EatController : MonoBehaviour
{
    PlayerStat stat;

    private void Start()
    {
        stat = GetComponentInParent<PlayerStat>();
    }

    public void EatActoin(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            UI_Item itemUI = Managers.Inven.currentItemUI;

            if (itemUI.slotInfo.itemInfo == null)
                return;

            if (itemUI.slotInfo.itemInfo.canEat)
            {
                stat.Hunger += itemUI.slotInfo.itemInfo.hungerAmount;
                itemUI.slotInfo.count--;
                if(itemUI.slotInfo.count == 0)
                    itemUI.SetEmptyItem();
            }
        }
    }
}
