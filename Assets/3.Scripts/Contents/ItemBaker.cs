using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBaker : MonoBehaviour
{
    ItemSO bakingItemSO;

    private void OnDisable()
    {
        Managers.Inven.AddOneItem(bakingItemSO.name);
    }

    public IEnumerator Bake(ItemSO item)
    {
        bakingItemSO = item;
        yield return new WaitForSeconds(item.bakeTime);
        MakeBakedItem(item.bakeItemSO);
    }

    void MakeBakedItem(ItemSO bakedItem)
    {
        BonFireController bonfire = GetComponentInParent<BonFireController>();
        bonfire.itemBakers.Add(this);
        bonfire.BakingCount--;
        Managers.Inven.AddOneItem(bakedItem.idName);
    }
}
