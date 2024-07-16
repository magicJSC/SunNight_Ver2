using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBaker : MonoBehaviour
{
   public IEnumerator Bake(ItemSO item)
    {
        yield return new WaitForSeconds(item.bakeTime);
        MakeBakedItem(item.bakeItemSO);
    }

    void MakeBakedItem(ItemSO bakedItem)
    {
        GetComponentInParent<BonFireController>().itemBakers.Add(this);
        Managers.Inven.AddOneItem(bakedItem.idName);
    }
}
