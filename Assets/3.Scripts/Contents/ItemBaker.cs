using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBaker : MonoBehaviour
{
    ItemSO bakingItemSO;

    private void OnDisable()
    {
        Managers.Inven.AddOneItem(bakingItemSO.name);
        Destroy(gameObject);
    }

    public IEnumerator Bake(ItemSO item)
    {
        bakingItemSO = item;
        yield return new WaitForSeconds(item.bakeTime);
        MakeBakedItem(item.bakeItemSO);
    }

    void MakeBakedItem(ItemSO bakedItem)
    {
        GetComponentInParent<BonFireController>().itemBakers.Add(this);
        Managers.Inven.AddOneItem(bakedItem.idName);
    }
}
