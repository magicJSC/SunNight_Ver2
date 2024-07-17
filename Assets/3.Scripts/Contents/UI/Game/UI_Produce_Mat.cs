using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Produce_Mat : UI_Base
{
    [HideInInspector]
    public UI_Produce produce;

    Image icon;

    Text count;

    GameObject explain;
    Text explainText;
    Text nameText;

    ItemSO itemInfo;

    public void Init(ItemSO itemSO,int cnt)
    {
        itemInfo = itemSO;

       icon = GetComponent<Image>();
        count = Util.FindChild(gameObject, "Count",true).GetComponent<Text>();

        explain = produce.explainMat;
        explainText = Util.FindChild(explain, "ExplainText", true).GetComponent<Text>();
        nameText = Util.FindChild(explain, "NameText", true).GetComponent<Text>();

        UI_EventHandler evt = icon.GetComponent<UI_EventHandler>();
        evt._OnEnter += (PointerEventData p) => { Set_Explain(); explain.SetActive(true); };
        evt._OnExit += (PointerEventData p) => { explain.SetActive(false); };


        icon.sprite = itemInfo.itemIcon;
        count.text = cnt.ToString();
    }

    public void Set_Explain()
    {
        explain.GetComponent<RectTransform>().anchoredPosition = GetComponent<RectTransform>().anchoredPosition + new Vector2(-390, 225);
        explainText.text = itemInfo.explain;
        nameText.text = itemInfo.itemName;
    }
}
