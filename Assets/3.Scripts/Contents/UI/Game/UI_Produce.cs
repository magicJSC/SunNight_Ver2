using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Xml.Schema;
using System;

public class UI_Produce : UI_Base
{
    public AudioClip produceSound;
    public AudioClip showSound;
    public AudioClip hideSound;

    [Serializable]
    public struct Materials
    {
        public ItemSO itemSO;
        public int count;
    }

    [Header("Produce")]
    //item1 : 재료 idName, item2 : 필요 개수
    public List<Materials> matters = new();
    [HideInInspector]
    public ItemSO toMakeItemSO;

    [Header("UI")]
    Image toMake;

    [HideInInspector]
    public GameObject back;
    GameObject contentMat;
    GameObject contentItem;
    GameObject produce;
    GameObject hide;
    [HideInInspector]
    public GameObject explainMat;
    [HideInInspector]
    public GameObject explainItem;

    Vector2 startPos;

    RectTransform backR;

    enum GameObjects
    {
        Background,
        Content_Mat,
        Content_Item,
        ToMake,
        Produce,
        Hide,
        Explain_Mat,
        Explain_Item
    }

    public override void Init()
    {
        if (_init)
            return;

        Bind<GameObject>(typeof(GameObjects));
        back = Get<GameObject>((int)GameObjects.Background);
        contentMat = Get<GameObject>((int)GameObjects.Content_Mat);
        contentItem = Get<GameObject>((int)GameObjects.Content_Item);
        toMake = Get<GameObject>((int)GameObjects.ToMake).GetComponent<Image>();
        produce = Get<GameObject>((int)GameObjects.Produce);
        hide = Get<GameObject>((int)GameObjects.Hide);
        explainMat = Get<GameObject>((int)GameObjects.Explain_Mat);
        explainItem = Get<GameObject>((int)GameObjects.Explain_Item);

        GetComponent<Canvas>().worldCamera = Camera.main;

        backR = back.GetComponent<RectTransform>();

        UI_EventHandler evt = back.GetComponent<UI_EventHandler>();
        evt._OnDrag += (PointerEventData p) =>
        {
            back.transform.position = new Vector3(Camera.main.ScreenToWorldPoint(p.position).x + startPos.x, Camera.main.ScreenToWorldPoint(p.position).y + startPos.y);
            Set_Position();
        };
        evt._OnDown += (PointerEventData p) => { startPos = new Vector3(back.transform.position.x - Camera.main.ScreenToWorldPoint(p.position).x, back.transform.position.y - Camera.main.ScreenToWorldPoint(p.position).y); };

        evt = produce.GetComponent<UI_EventHandler>();
        evt._OnClick += (PointerEventData p) => { OnProduce(); };

        evt = hide.GetComponent<UI_EventHandler>();
        evt._OnClick += (PointerEventData p) => { Managers.Inven.CheckHotBarChoice(); gameObject.SetActive(false); };

        for (int i = 0; i < contentItem.transform.childCount; i++)
        {
            UI_Produce_Item it = contentItem.transform.GetChild(i).GetComponent<UI_Produce_Item>();
            it.produce = this;
            it.Init();
        }

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
            Managers.Sound.Play(Define.Sound.Effect, hideSound);
    }

    public void Set_Position()
    {
        float x = Mathf.Clamp(backR.anchoredPosition.x, -665, 665);
        float y = Mathf.Clamp(backR.anchoredPosition.y, -135, 135);
        back.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
    }

    public void Set_ToMake(string itemName)
    {
        toMake.gameObject.SetActive(true);
        toMakeItemSO = Resources.Load<Item>($"Prefabs/Items/{itemName}").itemSo;
        toMake.sprite = toMakeItemSO.itemIcon;
        contentMat.GetComponent<RectTransform>().offsetMax = new Vector2(100 * matters.Count - 200, 0);
        for (int i = 0; i < matters.Count; i++)
        {
            UI_Produce_Mat ma = Instantiate(Resources.Load<GameObject>("UI/UI_Produce_Mat"), contentMat.transform).GetComponent<UI_Produce_Mat>();
            ma.produce = this;
            ma.Init(matters[i].itemSO, matters[i].count);
        }
    }

    public void Remove_ToMake()
    {
        toMake.gameObject.SetActive(false);
        for (int i = 0; i < contentMat.transform.childCount; i++)
        {
            Destroy(contentMat.transform.GetChild(i).gameObject);
        }
        matters.Clear();
    }


    //List -> item1 : 인벤 index, item2 : 필요 개수
    Dictionary<ItemSO, List<(UI_Item, int)>> inven_m = new();
    List<(UI_Item, int)> info_m;

    void OnProduce()
    {
        if (matters.Count == 0)
            return;

        if (CanProduce())
        {
            for (int i = 0; i < matters.Count; i++)
            {
                for (int j = 0; j < inven_m[matters[i].itemSO].Count; j++)
                {
                    int count = matters[i].count;
                    Managers.Inven.SetSlot(matters[i].itemSO, inven_m[matters[i].itemSO][j].Item1, Mathf.Clamp(info_m[j].Item2 - count, 0, matters[i].count));
                    count -= info_m[j].Item2;
                }
            }
            Managers.Inven.AddOneItem(toMakeItemSO.idName);
            Managers.Sound.Play(Define.Sound.Effect, produceSound);
        }
        else
            Debug.Log("재료가 부족합니다");
    }

    bool CanProduce()
    {
        inven_m.Clear();
        for (int i = 0; i < matters.Count; i++)
        {
            int _count = 0;
            for (int j = 0; j < Managers.Inven.inventorySlotInfo.Length - 1; j++)
            {
                UI_Item itemUI = Managers.Inven.inventoryUI.slotList[j].itemUI;
                if (itemUI.slotInfo.itemInfo == null)
                    continue;
                if (matters[i].itemSO == itemUI.slotInfo.itemInfo)
                {
                    _count += itemUI.slotInfo.count;
                    info_m = new()
                    {
                        (itemUI, itemUI.slotInfo.count)
                    };

                    if (_count >= matters[i].count)
                    {
                        inven_m.Add(matters[i].itemSO, info_m);
                        return true;
                    }
                }
            }
            for (int j = 0; j < Managers.Inven.hotBarSlotInfo.Length - 1; j++)
            {
                UI_Item itemUI = Managers.Inven.hotBarUI.slotList[j].itemUI;
                if (itemUI.slotInfo.itemInfo == null)
                    continue;
                if (matters[i].itemSO == itemUI.slotInfo.itemInfo)
                {
                    _count += itemUI.slotInfo.count;
                    info_m = new()
                    {
                        (itemUI, itemUI.slotInfo.count)
                    };

                    if (_count >= matters[i].count)
                    {
                        inven_m.Add(matters[i].itemSO, info_m);
                        return true;
                    }
                }
            }
        }
        return false;
    }
}
