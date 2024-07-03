using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_TowerSlot : UI_Base
{
    Image icon;


    public new void Init()
    {
        icon = Util.FindChild(gameObject,"Icon",true).GetComponent<Image>();
        GetComponent<Image>().color = Color.yellow;
        icon.sprite = Managers.Game.tower.GetComponent<SpriteRenderer>().sprite;
        HideTowerIcon();
    }

    public void ShowTowerIcon()
    {
        icon.gameObject.SetActive(true);
    }

    public void HideTowerIcon()
    {
        icon.gameObject.SetActive(false);
    }
}
