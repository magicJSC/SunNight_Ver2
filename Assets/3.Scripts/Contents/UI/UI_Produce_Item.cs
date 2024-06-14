using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static InvenManager;

public class UI_Produce_Item : UI_Base
{
    [Serializable]
    public struct matterInfo
    {
        public string matterName;
        public int count;
    }

    [Header("Produce")]
    public string itemName;
    public List<matterInfo> material = new List<matterInfo>();


    [HideInInspector]
    public UI_Produce produce;
    Item itemInfo;

    GameObject explain;
    Text explainText;
    Text nameText;

   

    public new void Init()
    {
        if (_init)
            return;

        _init = true;
        itemInfo = Resources.Load<Item>($"Prefabs/Items/{itemName}");
        explain = produce.explain;
        explainText = Util.FindChild(explain, "ExplainText", true).GetComponent<Text>();
        nameText = Util.FindChild(explain, "NameText", true).GetComponent<Text>();

        UI_EventHandler evt = GetComponent<UI_EventHandler>();
        evt._OnEnter += (PointerEventData p) => { explain.SetActive(true); Set_Explain(); };
        evt._OnExit += (PointerEventData p) => { explain.SetActive(false); };
        evt._OnClick += (PointerEventData p) => 
        {
            produce.Remove_ToMake();
            for(int i = 0; i < material.Count; i++)
            {
                produce.matters.Add((material[i].matterName, material[i].count));
            }
            produce.Set_ToMake(itemName); 
        };

        explain.SetActive(false);
    }

    void Set_Explain()
    {
        explainText.text = itemInfo.explain;
        nameText.text = itemInfo.itemName;
    }
}
