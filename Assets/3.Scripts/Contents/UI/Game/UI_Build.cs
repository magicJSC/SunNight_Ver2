using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UI_Build : MonoBehaviour
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

    //≥™¡ﬂø° Item_Buildingø° ¡§∫∏∑Œ ∞¸∏Æ «œ±‚∑Œ
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

    GameObject matGrid;
    public void Start()
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
        Managers.Game.buildUI = this;
    }

    private void OnDisable()
    {
        if (!Managers.HasInstance) return;
        Managers.UI.PopUIList.Remove(gameObject);
        Managers.Game.buildUI = null;
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

        coinT.text = $"∫ÒøÎ : {expension}";


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
        if (FieldSession.Active) { FieldHUD.Notify("Ï§ëÏïô ÏïºÏòÅÏßÄ ÏûëÏóÖÎåÄÏóêÏÑú Ïó∞Íµ¨ÌïòÏÑ∏Ïöî."); return; }
        if (nextLevelItem == null)
            return;

        if (Managers.Inven.Coin - expension < 0)
        {
            Debug.Log("µ∑ ∫Œ¡∑");
            return;
        }
        if (!CheckMaterials())
        {
            Debug.Log("¿Á∑· ∫Œ¡∑");
            return;
        }

        Debug.Log("æ˜±◊∑π¿ÃµÂ º∫∞¯");
        Managers.Inven.Coin -= expension;
        Vector2 tower = Managers.Game.tower.transform.position; //±‚¡ˆ ¿ßƒ° πﬁæ∆ø¿±‚
        MapManager.building.SetTile(new Vector3Int((int)(transform.parent.position.x - tower.x), (int)(transform.parent.position.y - tower.y), 0), nextLevelItem.buildTile);
    }

    void Collect(PointerEventData p) => TryCollectAction();

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

    public void TryCollectAction()
    {
        if (FieldFoxtonBattle.Active != null || Managers.Game.isMovingTower || TowerMoveController.IsHandling) return;
        var building = GetComponentInParent<Item_Buliding>();
        if (Managers.Inven.GetItem(building.itemSo, 1) != 0) return;
        Managers.Inven.CheckHotBarChoice();
        building.DeleteBuilding();
        Managers.Sound.Play(Define.Sound.Effect, clickSound);
    }
}
