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
    enum Texts
    {
        Count
    }

    GameObject explain;
    Text explainText;
    Text nameText;

    Item itemInfo;

    public void Init(string matterName,int _cnt)
    {
        itemInfo = Resources.Load<Item>($"Prefabs/Items/{matterName}");

        Bind<Text>(typeof(Texts));
       icon = GetComponent<Image>();
        count = Get<Text>((int)Texts.Count);
        
        explain = produce.explain;
        explainText = Util.FindChild(explain, "ExplainText", true).GetComponent<Text>();
        nameText = Util.FindChild(explain, "NameText", true).GetComponent<Text>();

        UI_EventHandler evt = icon.GetComponent<UI_EventHandler>();
        evt._OnEnter += (PointerEventData p) => { Set_Explain(); explain.SetActive(true); };
        evt._OnExit += (PointerEventData p) => { explain.SetActive(false); };


        icon.sprite = itemInfo.itemIcon;
        count.text = _cnt.ToString();
    }

    void Set_Explain()
    {
        explainText.text = itemInfo.explain;
        nameText.text = itemInfo.itemName;
    }
}
