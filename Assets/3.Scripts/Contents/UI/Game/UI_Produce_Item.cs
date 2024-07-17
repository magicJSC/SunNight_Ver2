using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Produce_Item : UI_Base
{

    [Header("Produce")]
    public ItemSO itemSO;
    public List<UI_Produce.Materials> materialList = new List<UI_Produce.Materials>();


    [HideInInspector]
    public UI_Produce produce;

    GameObject explain;
    Text explainText;
    Text nameText;


    public new void Init()
    {
        if (_init)
            return;

        _init = true;
        explain = produce.explainItem;
        explainText = Util.FindChild(explain, "ExplainText", true).GetComponent<Text>();
        nameText = Util.FindChild(explain, "NameText", true).GetComponent<Text>();

        UI_EventHandler evt = GetComponent<UI_EventHandler>();
        evt._OnEnter += (PointerEventData p) => { explain.SetActive(true); Set_Explain(); };
        evt._OnExit += (PointerEventData p) => { explain.SetActive(false); };
        evt._OnClick += (PointerEventData p) => 
        {
            produce.Remove_ToMake();
            for(int i = 0; i < materialList.Count; i++)
            {
                produce.matters.Add(materialList[i]);
            }
            produce.Set_ToMake(itemSO.idName); 
        };
    }

    public void Set_Explain()
    {
        explain.GetComponent<RectTransform>().anchoredPosition = GetComponent<RectTransform>().anchoredPosition + new Vector2(-515, 450);
        explainText.text = itemSO.explain;
        nameText.text = itemSO.itemName;
    }
}
