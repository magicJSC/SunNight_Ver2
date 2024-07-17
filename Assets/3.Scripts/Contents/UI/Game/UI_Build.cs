using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Build : UI_Base
{
    public AudioClip clickSound;
    public AudioClip upgradeSound;

    [Serializable]
    public class Matters
    {
        public ItemSO item;
        public int count;
    }

    //나중에 Item_Building에 정보로 관리 하기로
    public List<Matters> matters = new List<Matters>();

    public int expension;

    public ItemSO nextLevelItem;

    Item_Buliding itemData;
    BuildStat buildStat;


    Text nameT;
    Text hpT;
    Text dmgT;
    Text atkCoolT;
    Text rangeT;
    Text coinT;

    Image icon;
    Image upgrade;
    Image close;
    Image collect;

    GameObject panel;
    GameObject matGrid;

    enum Texts
    {
        Name,
        Hp,
        Dmg,
        AtkCool,
        Range,
        Coin
    }

    enum Images 
    {
        Icon,
        Upgrade,
        Close,
        Collect
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
        coinT = Get<Text>((int)Texts.Coin);
        
        icon = Get<Image>((int)Images.Icon);
        upgrade = Get<Image>((int)Images.Upgrade);
        close = Get<Image>((int)Images.Close);
        collect = Get<Image>((int)Images.Collect);

        matGrid = Util.FindChild(gameObject,"MatterGrid",true);
        panel = Util.FindChild(gameObject,"Panel",true);

        itemData = transform.GetComponentInParent<Item_Buliding>();
        buildStat = transform.GetComponentInParent<BuildStat>();

        UI_EventHandler evt = upgrade.GetComponent<UI_EventHandler>();
        evt._OnClick += UpgradeStat;

        evt = close.GetComponent<UI_EventHandler>();
        evt._OnClick += Close;

        evt = collect.GetComponent<UI_EventHandler>();
        evt._OnClick += Collect;

        InitData();
        gameObject.SetActive(false);
    }

    private void OnDisappear()
    {
        Managers.Game.isHandleUI = false;
        Managers.Inven.CheckHotBarChoice();
        gameObject.SetActive(false);
        Managers.Sound.Play(Define.Sound.Effect, clickSound);
        Managers.Game.canHandleMenuUI = true;
    }

    void InitData()
    {
        nameT.text = $"{itemData.itemSo.idName}";
        hpT.text = $"{buildStat.Hp}";

        if (buildStat.Damage != 0)
            dmgT.text = $"{buildStat.Damage}";
        else
            dmgT.text = "-";
        if (buildStat.attackCool != 0)
            atkCoolT.text = $"{buildStat.attackCool}";
        else
            atkCoolT.text = "-";
        if(buildStat.range != 0)
            rangeT.text = $"{buildStat.range}";
        else
            rangeT.text = "-";

        icon.sprite = itemData.itemSo.itemIcon;

        coinT.text = $"비용 : {expension}";

        for (int i = 0; i < matters.Count; i++)
        {
            GameObject go = Instantiate(Resources.Load<GameObject>("UI/UI_Build_Matter"), matGrid.transform);
            go.GetComponent<Image>().sprite = matters[i].item.itemIcon;
            go.transform.GetComponentInChildren<Text>().text = $"{matters[i].count}";
        }
    }

    void UpgradeStat(PointerEventData p)
    {
        if (nextLevelItem == null)
            return;

        if (Managers.Inven.Coin - expension < 0)
        {
            Debug.Log("돈 부족");
            return; 
        }
        if (!CheckMaterials())
        {
            Debug.Log("재료 부족");
            return;
        }

        Debug.Log("업그레이드 성공");
        Managers.Inven.Coin -= expension;
        Vector2 tower = Managers.Game.tower.transform.position; //기지 위치 받아오기
        MapManager.building.SetTile(new Vector3Int((int)(transform.parent.position.x - tower.x), (int)(transform.parent.position.y - tower.y), 0), nextLevelItem.tile);
    }

    void Collect(PointerEventData p)
    {
        Item_Buliding building = GetComponentInParent<Item_Buliding>();
        Managers.Inven.AddItems(building.itemSo.idName,1);
        Managers.Inven.CheckHotBarChoice();
        Managers.Game.isHandleUI = false;
        building.DeleteBuilding();
        Managers.Sound.Play(Define.Sound.Effect, clickSound);
    }

    void Close(PointerEventData p)
    {
        Managers.Game.isHandleUI = false;
        Managers.Inven.CheckHotBarChoice(); 
        gameObject.SetActive(false);
        Managers.Sound.Play(Define.Sound.Effect, clickSound);
        Managers.Game.canHandleMenuUI = true;
    }

    List<(UI_Item,int)> ItemUIList = new List<(UI_Item, int)>();

    bool CheckMaterials()
    {
        UI_InventorySlot[] invenInfos = Managers.Inven.inventoryUI.slotList;
        for (int i = 0; i < matters.Count; i++)
        {
            int count = matters[i].count;
            for (int j = 0; j < invenInfos.Length; j++)
            {
                UI_Item itemUI = invenInfos[j].itemUI;
                if (itemUI.slotInfo.itemInfo == matters[i].item)
                {
                    if(count - itemUI.slotInfo.count <= 0)
                    {
                        int remain = Mathf.Clamp(itemUI.slotInfo.count - count, 0, itemUI.slotInfo.count);
                        ItemUIList.Add((itemUI,remain));
                        count -= itemUI.slotInfo.count;
                        break;
                    }
                    count -= itemUI.slotInfo.count;
                }
            }
            if (count > 0)
                return false;
        }

        UseMaterials();
        return true;
    }

    void UseMaterials()
    {
        for(int i = 0; i < ItemUIList.Count; i++)
        {
            if (ItemUIList[i].Item2 == 0)
                ItemUIList[i].Item1.MakeEmptySlot();
            else
                ItemUIList[i].Item1.slotInfo.count = ItemUIList[i].Item2;
        }
        ItemUIList.Clear();
    }
}
