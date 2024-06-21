using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_TowerSlot : UI_Base
{
    Image icon;

    enum Images
    {
        Icon
    }

    public new void Init()
    {
        Bind<Image>(typeof(Images));
        icon = Get<Image>((int)Images.Icon);

        GetComponent<Image>().color = Color.yellow;
        //icon.sprite = Managers.Game.tower.GetComponent<SpriteRenderer>().sprite;
        Hide_Tower_Icon();
    }

    public void Show_Tower_Icon()
    {
        icon.gameObject.SetActive(true);
    }

    public void Hide_Tower_Icon()
    {
        icon.gameObject.SetActive(false);
    }
}
