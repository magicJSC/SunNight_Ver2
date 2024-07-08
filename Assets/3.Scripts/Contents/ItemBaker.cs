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
        Debug.Log("아이템을 구웠습니다");
        GetComponentInParent<BonFireController>().itemBakers.Add(this);
        //bakedItem.bakeItemSO;
    }
}
