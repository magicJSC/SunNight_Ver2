using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Produce_Material : UI_Base
{
    [HideInInspector]
    public UI_Produce produce;

    Image icon;

    Text count;

    RectTransform explain;
    RectTransform rectTransform;
    Text explainText;
    Text nameText;

    public UI_Produce.Materials material;

    public override void Init()
    {
        icon = GetComponent<Image>();
        count = Util.FindChild<Text>(gameObject, "Count",true);
        rectTransform = GetComponent<RectTransform>();
        explain = produce.explainMat.GetComponent<RectTransform>();
        explainText = Util.FindChild<Text>(explain.gameObject, "ExplainText", true);
        nameText = Util.FindChild<Text>(explain.gameObject, "NameText", true);

        UI_EventHandler evt = icon.GetComponent<UI_EventHandler>();
        evt._OnEnter += (PointerEventData p) => { Set_Explain(); explain.gameObject.SetActive(true); };
        evt._OnExit += (PointerEventData p) => { explain.gameObject.SetActive(false); };


        icon.sprite = material.itemSO.itemIcon;
        count.text = $"{material.count}";
    }

    public void Set_Explain()
    {
        explain.anchoredPosition = rectTransform.anchoredPosition + new Vector2(-390, 225);
        explainText.text = material.itemSO.explain;
        nameText.text = material.itemSO.itemName;
    }
}
