using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class EatController : MonoBehaviour
{
    PlayerStat stat;
    float nextPotionTime;

    private void Start()
    {
        stat = GetComponentInParent<PlayerStat>();
    }

    public void EatActoin(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (FieldHUD.IsOpen || Managers.Game.isCantPlay || PlayerController.isDie) return;
            UI_Item itemUI = Managers.Inven.currentItemUI;
            if (itemUI == null || itemUI.slotInfo == null) return;

            if (itemUI.slotInfo.itemInfo == null)
                return;

            if (itemUI.slotInfo.itemInfo.idName == "HpPotion")
            {
                if (Time.time < nextPotionTime || stat.Hp >= stat.maxHP) return;
                stat.Hp += 20; nextPotionTime = Time.time + 10;
                itemUI.slotInfo.count--; itemUI.SetInfo();
                return;
            }
            if (itemUI.slotInfo.itemInfo.canEat)
            {
                if (stat.Hunger >= stat.maxHunger) return;
                stat.Hunger += itemUI.slotInfo.itemInfo.hungerAmount;
                itemUI.slotInfo.count--;
                itemUI.SetInfo();
                if (itemUI.slotInfo.count == 0)
                {
                    itemUI.MakeEmptySlot();
                    Managers.Inven.CheckHotBarChoice();
                }
            }
        }
    }
}
