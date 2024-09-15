using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Build : UI_Base
{
    public AssetReferenceT<AudioClip> clickSoundAsset;
    public AssetReferenceT<AudioClip> upgradeSoundAsset;

    public AssetReferenceGameObject buildMatterUIAsset;

    AudioClip clickSound;
    AudioClip upgradeSound;

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


    public override void Init()
    {

        nameT = Util.FindChild<Text>(gameObject, "Name", true);
        hpT = Util.FindChild<Text>(gameObject, "Hp", true);
        dmgT = Util.FindChild<Text>(gameObject, "Dmg", true);
        atkCoolT = Util.FindChild<Text>(gameObject, "AtkCool", true);
        rangeT = Util.FindChild<Text>(gameObject, "Range", true);
        coinT = Util.FindChild<Text>(gameObject, "Coin", true);

        icon = Util.FindChild<Image>(gameObject, "Icon", true);
        upgrade = Util.FindChild<Image>(gameObject, "Upgrade", true);
        close = Util.FindChild<Image>(gameObject, "Close", true);
        collect = Util.FindChild<Image>(gameObject, "Collect", true);

        matGrid = Util.FindChild(gameObject, "MatterGrid", true);
        panel = Util.FindChild(gameObject, "Panel", true);

        itemData = transform.GetComponentInParent<Item_Buliding>();
        buildStat = transform.GetComponentInParent<BuildStat>();

        UI_EventHandler evt = upgrade.GetComponent<UI_EventHandler>();
        evt._OnClick += UpgradeStat;

        evt = close.GetComponent<UI_EventHandler>();
        evt._OnClick += Close;

        evt = collect.GetComponent<UI_EventHandler>();
        evt._OnClick += Collect;

        clickSoundAsset.LoadAssetAsync().Completed += (clip) =>
        {
            clickSound = clip.Result;
        };
        upgradeSoundAsset.LoadAssetAsync().Completed += (clip) =>
        {
            upgradeSound = clip.Result;
        };

        InitData();
        gameObject.SetActive(false);
        
    }

    private void OnEnable()
    {
        Managers.UI.PopUIList.Add(gameObject);
    }

    private void OnDisable()
    {
        Managers.UI.PopUIList.Remove(gameObject);
    }

    public void Disappear()
    {
        Managers.Game.isHandleUI = false;
        Managers.Inven.CheckHotBarChoice();
        gameObject.SetActive(false);
        Managers.Sound.Play(Define.Sound.Effect, clickSound);
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
        if (buildStat.range != 0)
            rangeT.text = $"{buildStat.range}";
        else
            rangeT.text = "-";

        icon.sprite = itemData.itemSo.itemIcon;

        coinT.text = $"비용 : {expension}";


        buildMatterUIAsset.LoadAssetAsync().Completed += (obj) =>
        {
            for (int i = 0; i < matters.Count; i++)
            {
                GameObject go = Instantiate(obj.Result,matGrid.transform);
                go.GetComponent<Image>().sprite = matters[i].item.itemIcon;
                go.transform.GetComponentInChildren<Text>().text = $"{matters[i].count}";
            }
        };

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
        MapManager.building.SetTile(new Vector3Int((int)(transform.parent.position.x - tower.x), (int)(transform.parent.position.y - tower.y), 0), nextLevelItem.buildTile);
    }

    void Collect(PointerEventData p)
    {
        Item_Buliding building = GetComponentInParent<Item_Buliding>();
        Managers.Inven.AddItems(building.itemSo, 1);
        Managers.Inven.CheckHotBarChoice();
        Managers.Game.isHandleUI = false;
        building.DeleteBuilding();
        Managers.Sound.Play(Define.Sound.Effect, clickSound);
    }

    void Close(PointerEventData p)
    {
        Disappear();
    }

    List<(UI_Item, int)> ItemUIList = new List<(UI_Item, int)>();

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
                    if (count - itemUI.slotInfo.count <= 0)
                    {
                        int remain = Mathf.Clamp(itemUI.slotInfo.count - count, 0, itemUI.slotInfo.count);
                        ItemUIList.Add((itemUI, remain));
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
        for (int i = 0; i < ItemUIList.Count; i++)
        {
            if (ItemUIList[i].Item2 == 0)
                ItemUIList[i].Item1.MakeEmptySlot();
            else
                ItemUIList[i].Item1.slotInfo.count = ItemUIList[i].Item2;
        }
        ItemUIList.Clear();
    }
}
