using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Produce_Item : UI_Base
{
    [Header("Produce")]
    public UI_Produce.ToMakeItem toMakeItem;


    [HideInInspector]
    public UI_Produce produce;

    Image icon;

    RectTransform explain;
    RectTransform rectTransform;

    Text explainText;
    Text nameText;
    

    public override void Init()
    {
        explain = produce.explainItem.GetComponent<RectTransform>();
        rectTransform = GetComponent<RectTransform>();
        explainText = Util.FindChild<Text>(explain.gameObject, "ExplainText", true);
        nameText = Util.FindChild<Text>(explain.gameObject, "NameText", true);

        icon = Util.FindChild<Image>(gameObject, "Icon", true);
        icon.sprite = toMakeItem.toMakeItemSO.itemIcon;

        UI_EventHandler evt = GetComponent<UI_EventHandler>();
        evt._OnEnter += (PointerEventData p) => { explain.gameObject.SetActive(true); Set_Explain(); };
        evt._OnExit += (PointerEventData p) => { explain.gameObject.SetActive(false); };
        evt._OnClick += (PointerEventData p) =>
        {
            produce.Remove_ToMake();
            produce.toMakeItem = toMakeItem;
            produce.Set_ToMake(toMakeItem);
        };
    }

    public void Set_Explain()
    {
        explain.anchoredPosition = rectTransform.anchoredPosition + new Vector2(-515, 450);
        explainText.text = toMakeItem.toMakeItemSO.explain;
        nameText.text = toMakeItem.toMakeItemSO.itemName;
    }
}
