using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemSign : MonoBehaviour
{
    public void SetItem(ItemSO itemSo, int count)
    {
        Util.FindChild<Image>(gameObject,"Item",true).sprite = itemSo.itemIcon;
        if(count > 1)
         Util.FindChild<Text>(gameObject,"Count",true).text =$"X{count}";
        else
            Util.FindChild(gameObject, "Count", true).SetActive(false);
    }

    public void Disappear()
    {
        Destroy(gameObject);
    }
}
