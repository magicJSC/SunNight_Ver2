using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBaker : MonoBehaviour
{

    public IEnumerator Bake(ItemSO item)
    {
        yield return new WaitForSeconds(item.bakeTime);
        MakeBakedItem(item.bakeItem);
    }

    void MakeBakedItem(ItemSO bakedItem)
    {
        BonFireController bonfire = GetComponentInParent<BonFireController>();
        bonfire.itemBakers.Add(this);
        bonfire.BakingCount--;
        Vector2 pos = bonfire.transform.position + Managers.Game.tower.transform.position;
        Managers.Game.GetItem(bakedItem,1,pos);
    }
}
