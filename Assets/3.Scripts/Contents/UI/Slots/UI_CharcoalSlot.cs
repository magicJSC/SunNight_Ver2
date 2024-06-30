using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_CharcoalSlot : UI_BaseSlot
{
    public int charcoalCount;

    Image icon;
    Text count;

    public new void Init()
    {
        icon = Util.FindChild(gameObject, "Icon",true).GetComponent<Image>();
        count = Util.FindChild(gameObject, "Count",true).GetComponent<Text>();
        icon.sprite = Resources.Load<Item>("Prefabs/Items/Coal").itemSo.itemIcon; 
    }

    public void SetSlot()
    {
        if (charcoalCount == 0)
        {
            SetEmptySlot();
            return;
        }
        count.text = charcoalCount.ToString();
    }

    public void SetExistSlot()
    {
        icon.gameObject.SetActive(true);
        count.gameObject.SetActive(charcoalCount != 1);
    }

    void SetEmptySlot()
    {
        icon.gameObject.SetActive(false);
        count.gameObject.SetActive(false);
    }
}
