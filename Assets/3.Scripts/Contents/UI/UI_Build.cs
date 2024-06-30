using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Build : UI_Base
{
    [Serializable]
    public class Matters
    {
        public string itemName;
        public int count;
    }

    //나중에 Item_Building에 정보로 관리 하기로
    public List<Matters> matters = new List<Matters>();

    public int coin;

    Item_Buliding itemData;
    BuildStat buildStat;

    Text nameT;
    Text hpT;
    Text dmgT;
    Text atkCoolT;
    Text rangeT;

    Image icon;
    Image upgrade;
    Image close;
    Image background;

    GameObject panel;
    GameObject matGrid;

    enum Texts
    {
        Name,
        Hp,
        Dmg,
        AtkCool,
        Range
    }

    enum Images 
    {
        Icon,
        Upgrade,
        Close,
    }

    public override void Init()
    {
        Bind<Image>(typeof(Images));
        Bind<Text>(typeof(Texts));

        nameT = Get<Text>((int)Texts.Name);
        hpT = Get<Text>((int)Texts.Hp);
        dmgT = Get<Text>((int)Texts.Dmg);
        atkCoolT = Get<Text>((int)Texts.AtkCool);
        rangeT = Get<Text>((int)Texts.Range);
        
        icon = Get<Image>((int)Images.Icon);
        upgrade = Get<Image>((int)Images.Upgrade);
        close = Get<Image>((int)Images.Close);

        matGrid = Util.FindChild(gameObject,"MatterGrid",true);
        panel = Util.FindChild(gameObject,"Panel",true);

        itemData = transform.parent.GetComponent<Item_Buliding>();
        buildStat = transform.parent.GetComponent<BuildStat>();

        UI_EventHandler evt = upgrade.GetComponent<UI_EventHandler>();
        evt._OnClick += (PointerEventData p) => {  };

        evt = close.GetComponent<UI_EventHandler>();
        evt._OnClick += Close;

        evt = panel.GetComponent<UI_EventHandler>();
        evt._OnEnter += (PointerEventData p) => 
        {
            Managers.Game.isHandleUI = true;
            Managers.Game.mouse.CursorType = Define.CursorType.Normal; 
        };

        InitData();
        gameObject.SetActive(false);
    }

    void InitData()
    {
        nameT.text = $"{itemData.itemSo.idName}";
        hpT.text = buildStat.Hp.ToString();

        if (buildStat.Damage != 0)
            dmgT.text = buildStat.Damage.ToString();
        else
            dmgT.text = "-";
        if (buildStat.attackCool != 0)
            atkCoolT.text = buildStat.attackCool.ToString();
        else
            atkCoolT.text = "-";
        if(buildStat.range != 0)
            rangeT.text = buildStat.range.ToString();
        else
            rangeT.text = "-";

        icon.sprite = itemData.itemSo.itemIcon;

        for (int i = 0; i < matters.Count; i++)
        {
            GameObject go = Instantiate(Resources.Load<GameObject>("UI/UI_Build_Matter"), matGrid.transform);
            go.GetComponent<Image>().sprite = Resources.Load<Item>($"Prefabs/Items/{matters[i].itemName}").itemSo.itemIcon;
            go.transform.GetChild(0).GetComponent<Text>().text = matters[i].count.ToString();
        }
    }

    void UpgradeStat()
    {

    }

    void Close(PointerEventData p)
    {
        Managers.Game.isHandleUI = false;
        Managers.Inven.CheckHotBarChoice(); 
        gameObject.SetActive(false);
    }

    //bool Check_Materials()
    //{
    //    for(int i=0;i<matters.Count;i++)
    //    {
    //        Check_Materials_Inven();
    //    }

    //    return true;
    //}

    //bool Check_Materials_Inven()
    //{

    //}
}
