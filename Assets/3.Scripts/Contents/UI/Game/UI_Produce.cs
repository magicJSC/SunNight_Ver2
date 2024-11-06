using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using UnityEngine.AddressableAssets;

public class UI_Produce : UI_Base
{
    public AssetReferenceGameObject itemUIAsset;

    GameObject itemUI;

    public AssetReferenceT<AudioClip> produceSoundAsset;
    public AssetReferenceT<AudioClip> showSoundAsset;
    public AssetReferenceT<AudioClip> hideSoundAsset;

    AudioClip produceSound;
    AudioClip showSound;
    AudioClip hideSound;

    public AssetReferenceGameObject produceMaterialUIAsset;

    GameObject produceMaterialUI;
    [HideInInspector]
    public UI_Produce_Item produceItemUI;


    [Serializable]
    public struct ToMakeItem
    {
        public ItemSO toMakeItemSO;
        public Materials[] materialList;
    }

    [Serializable]
    public struct Materials
    {
        public ItemSO itemSO;
        public int count;
    }

    [Header("Produce")]
    //item1 : 재료 idName, item2 : 필요 개수
    public List<ToMakeItem> toMakeItemList = new();

    [HideInInspector]
    public ToMakeItem toMakeItem;

    [Header("UI")]
    Image toMake;

    [HideInInspector]
    public GameObject back;
    RectTransform contentMat;
    GameObject contentItem;
    Image produce;
    RectTransform hideRect;
    [HideInInspector]
    public GameObject explainMat;
    [HideInInspector]
    public GameObject explainItem;

    Text produceButtonText;

    Vector2 startPos;

    RectTransform backR;

    bool canProduce;

    public override void Init()
    {
        if (_init)
            return;

        back = Util.FindChild(gameObject, "Background", true);
        contentMat = Util.FindChild<RectTransform>(gameObject, "Content_Mat", true);
        contentItem = Util.FindChild(gameObject, "Content_Item", true);
        toMake = Util.FindChild<Image>(gameObject, "ToMake", true);
        produce = Util.FindChild<Image>(gameObject, "Produce", true);
        hideRect = Util.FindChild<RectTransform>(gameObject, "Hide", true);
        explainMat = Util.FindChild(gameObject, "Explain_Mat", true);
        explainItem = Util.FindChild(gameObject, "Explain_Item", true);
        produceButtonText = Util.FindChild(gameObject, "ProduceText", true).GetComponent<Text>();

        GetComponent<Canvas>().worldCamera = Camera.main;

        backR = back.GetComponent<RectTransform>();

        produceMaterialUIAsset.LoadAssetAsync().Completed += (obj) =>
        {
            produceMaterialUI = obj.Result;
        };
        itemUIAsset.LoadAssetAsync().Completed += (obj) =>
        {
            itemUI = obj.Result;
            for (int i = 0; i < toMakeItemList.Count; i++)
            {
                UI_Produce_Item item = Instantiate(itemUI, contentItem.transform).GetComponent<UI_Produce_Item>();
                item.produce = this;
                item.toMakeItem = toMakeItemList[i];
            }
        };

        UI_EventHandler evt = back.GetComponent<UI_EventHandler>();
        evt._OnDrag += (PointerEventData p) =>
        {
            back.transform.position = new Vector3(Camera.main.ScreenToWorldPoint(p.position).x + startPos.x, Camera.main.ScreenToWorldPoint(p.position).y + startPos.y);
            Set_Position();
        };
        evt._OnDown += (PointerEventData p) => { startPos = new Vector3(back.transform.position.x - Camera.main.ScreenToWorldPoint(p.position).x, back.transform.position.y - Camera.main.ScreenToWorldPoint(p.position).y); };

        evt = produce.GetComponent<UI_EventHandler>();
        evt._OnClick += (PointerEventData p) => { OnProduce(); };
        evt._OnEnter += EnterProduceButton;
        evt._OnExit += ExitProduceButton;

        evt = hideRect.GetComponent<UI_EventHandler>();
        evt._OnClick += (PointerEventData p) => { gameObject.SetActive(false); };

        produceSoundAsset.LoadAssetAsync().Completed += (clip) =>
        {
            produceSound = clip.Result;
        };
        hideSoundAsset.LoadAssetAsync().Completed += (clip) =>
        {
            hideSound = clip.Result;
        };
        showSoundAsset.LoadAssetAsync().Completed += (clip) =>
        {
            showSound = clip.Result;
        };

        Remove_ToMake();
        explainMat.SetActive(false);
        explainItem.SetActive(false);
        gameObject.SetActive(false);
        _init = true;
    }

    private void OnEnable()
    {
        if (_init)
        {
            Remove_ToMake();
            Managers.Sound.Play(Define.Sound.Effect, showSound);
        }
    }

    private void OnDisable()
    {
        if (_init)
        {
            Managers.Sound.Play(Define.Sound.Effect, hideSound);
            explainItem.SetActive(false);
            explainMat.SetActive(false);
        }
    }

    public void Set_Position()
    {
        float x = Mathf.Clamp(backR.anchoredPosition.x, -665, 665);
        float y = Mathf.Clamp(backR.anchoredPosition.y, -135, 135);
        backR.anchoredPosition = new Vector2(x, y);
    }

    public void Set_ToMake(ToMakeItem toMakeItem)
    {
        toMake.gameObject.SetActive(true);
        toMake.sprite = toMakeItem.toMakeItemSO.itemIcon;
        contentMat.offsetMax = new Vector2(100 * toMakeItem.materialList.Length - 200, 0);

        for (int i = 0; i < toMakeItem.materialList.Length; i++)
        {
            UI_Produce_Material ma = Instantiate(produceMaterialUI, contentMat.transform).GetComponent<UI_Produce_Material>();
            ma.produce = this;
            ma.material = toMakeItem.materialList[i];
        }
        if (!CanProduce())
        {
            produce.color = new Color(0.5f,0.5f,0.5f);
            canProduce = false;
        }
        else
        {
            produce.color = new Color(1f, 1f, 1f);
            canProduce = true;
        }
    }

    public void Remove_ToMake()
    {
        toMake.gameObject.SetActive(false);
        for (int i = 0; i < contentMat.transform.childCount; i++)
        {
            Destroy(contentMat.transform.GetChild(i).gameObject);
        }
    }


    //List -> item1 : 인벤 index, item2 : 필요 개수
    Dictionary<string, List<UI_Item>> itemUIList = new();
    void OnProduce()
    {
        if (toMakeItem.materialList.Length == 0)
            return;
        if (!canProduce)
            return;
        for (int i = 0; i < toMakeItem.materialList.Length; i++)
        {
            List<UI_Item> _itemUIList = itemUIList[toMakeItem.materialList[i].itemSO.itemName];
            int count = toMakeItem.materialList[i].count;
            for (int j = 0; j < _itemUIList.Count; j++)
            {
                int amount = _itemUIList[j].slotInfo.count;
                Managers.Inven.SetSlot(toMakeItem.materialList[i].itemSO, _itemUIList[j], Mathf.Clamp(_itemUIList[j].slotInfo.count - count, 0, _itemUIList[j].slotInfo.count));
                count -= amount;
            }
        }
        Managers.Inven.GetItem(toMakeItem.toMakeItemSO, 1);
        if (produceSound != null)
            Managers.Sound.Play(Define.Sound.Effect, produceSound);

        if (!CanProduce())
        {
            produce.color = new Color(0.5f, 0.5f, 0.5f);
            canProduce = false;
        }
        else
        {
            produce.color = new Color(1f, 1f, 1f);
            canProduce = true;
        }
    }

    void EnterProduceButton(PointerEventData p)
    {
        if(canProduce)
            produceButtonText.color = Color.red;
    }

    void ExitProduceButton(PointerEventData p)
    {
        produceButtonText.color = Color.black;
    }

    bool CanProduce()
    {
        itemUIList.Clear();
        for (int i = 0; i < toMakeItem.materialList.Length; i++)
        {
            int count = 0;
            bool canProduce = false;
            itemUIList.Add(toMakeItem.materialList[i].itemSO.itemName, new());
            for (int j = 0; j < 24; j++)
            {
                if (canProduce)
                    break;
                UI_Item itemUI = Managers.Inven.inventoryUI.slotList[j].itemUI;
                if (itemUI.slotInfo.itemInfo == null)
                    continue;

                if (toMakeItem.materialList[i].itemSO.idName == itemUI.slotInfo.itemInfo.idName)
                {
                    count += itemUI.slotInfo.count;
                    itemUIList[itemUI.slotInfo.itemInfo.itemName].Add(itemUI);

                    if (count >= toMakeItem.materialList[i].count)
                        canProduce = true;
                }
            }
            for (int j = 0; j < 4; j++)
            {
                if (canProduce)
                    break;
                UI_Item itemUI = Managers.Inven.hotBarUI.slotList[j].itemUI;
                if (itemUI.slotInfo.itemInfo == null)
                    continue;
                if (toMakeItem.materialList[i].itemSO == itemUI.slotInfo.itemInfo)
                {
                    count += itemUI.slotInfo.count;
                    itemUIList[itemUI.slotInfo.itemInfo.itemName].Add(itemUI);

                    if (count >= toMakeItem.materialList[i].count)
                        canProduce = true;
                }
            }
            if (!canProduce)
                return false;
        }
        return true;
    }
}
