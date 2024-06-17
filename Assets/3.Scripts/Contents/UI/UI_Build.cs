using System;
using System.Collections;
using System.Collections.Generic;
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
        Background
    }

    enum GameObjects
    {
        MatterGrid
    }

    public override void Init()
    {
        Bind<Image>(typeof(Images));
        Bind<Text>(typeof(Texts));
        Bind<GameObject>(typeof(GameObjects));

        nameT = Get<Text>((int)Texts.Name);
        hpT = Get<Text>((int)Texts.Hp);
        dmgT = Get<Text>((int)Texts.Dmg);
        atkCoolT = Get<Text>((int)Texts.AtkCool);
        rangeT = Get<Text>((int)Texts.Range);
        
        icon = Get<Image>((int)Images.Icon);
        upgrade = Get<Image>((int)Images.Upgrade);
        close = Get<Image>((int)Images.Close);
        background = Get<Image>((int)Images.Background);

        matGrid = Get<GameObject>((int)GameObjects.MatterGrid);

        itemData = transform.parent.GetComponent<Item_Buliding>();
        buildStat = transform.parent.GetComponent<BuildStat>();

        UI_EventHandler evt = upgrade.GetComponent<UI_EventHandler>();
        evt._OnClick += (PointerEventData p) => {  };

        evt = close.GetComponent<UI_EventHandler>();
        evt._OnClick += (PointerEventData p) => { Managers.Inven.Set_HotBar_Choice(); Destroy(gameObject); };

        evt = background.GetComponent<UI_EventHandler>();
        evt._OnEnter += (PointerEventData p) => 
        {
            Managers.Game.mouse.CursorType = Define.CursorType.Normal; 
        };
        evt._OnExit += (PointerEventData p) =>
        {
            Managers.Inven.Set_HotBar_Choice();
        };

        InitData();
    }

    void InitData()
    {
        nameT.text = $"{itemData.idName}";
        hpT.text = buildStat.Hp.ToString();

        if (buildStat.Dmg != 0)
            dmgT.text = buildStat.Dmg.ToString();
        else
            dmgT.text = "-";
        if (buildStat._atkCool != 0)
            atkCoolT.text = buildStat._atkCool.ToString();
        else
            atkCoolT.text = "-";
        if(buildStat._range != 0)
            rangeT.text = buildStat._range.ToString();
        else
            rangeT.text = "-";

        icon.sprite = itemData.itemIcon;

        for (int i = 0; i < matters.Count; i++)
        {
            GameObject go = Instantiate(Resources.Load<GameObject>("UI/UI_Build_Matter"), matGrid.transform);
            go.GetComponent<Image>().sprite = Resources.Load<Item>($"Prefabs/Items/{matters[i].itemName}").itemIcon;
            go.transform.GetChild(0).GetComponent<Text>().text = matters[i].count.ToString();
        }
    }

    void UpgradeStat()
    {

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
