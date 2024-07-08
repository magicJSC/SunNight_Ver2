using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HpPotion : Item, IConsumable
{
    public void Consume()
    {
        Managers.Game.player.GetComponent<PlayerStat>().Hp += 20;
    }
}
