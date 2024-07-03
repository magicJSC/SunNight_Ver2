using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

public class UI_Produce : UI_Base
{
    [Header("Produce")]
    //item1 : 재료 idName, item2 : 필요 개수
    public List<(string,int)> matters = new();
    public string toMakeIDName;

    [Header("UI")]
    Image toMake;

    public GameObject back;
    GameObject contentMat;
    GameObject contentItem;
    GameObject produce;
    GameObject hide;
    public GameObject explainMat;
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

        _init = true;
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
        evt._OnEnter += (PointerEventData p) => 
        {
            Managers.Game.isHandleUI = true;
            if(Managers.Game.mouse.CursorType != Define.CursorType.Drag)
            Managers.Game.mouse.CursorType = Define.CursorType.Normal;
        };
        evt._OnExit += (PointerEventData p) =>
        {
            Managers.Game.isHandleUI = false;
            if (Managers.Game.mouse.CursorType != Define.CursorType.Drag)
                Managers.Inven.CheckHotBarChoice(); 
        };

        evt = produce.GetComponent<UI_EventHandler>();
        evt._OnClick += (PointerEventData p) => { OnProduce(); };

        evt = hide.GetComponent<UI_EventHandler>();
        evt._OnClick += (PointerEventData p) => { Managers.Game.isHandleUI = false; gameObject.SetActive(false); };

        for(int i = 0; i < contentItem.transform.childCount; i++)
        {
            UI_Produce_Item it = contentItem.transform.GetChild(i).GetComponent<UI_Produce_Item>();
            it.produce = this;
            it.Init();
        }

        Remove_ToMake();
        explainMat.SetActive(false);
        explainItem.SetActive(false);
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if(_init)
            Remove_ToMake();    
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
        toMake.sprite = Resources.Load<Item>($"Prefabs/Items/{itemName}").itemSo.itemIcon;
        toMakeIDName = itemName;
        contentMat.GetComponent<RectTransform>().offsetMax = new Vector2(100 * matters.Count -200, 0);
        for( int i = 0; i < matters.Count; i++)
        {
            UI_Produce_Mat ma = Instantiate(Resources.Load<GameObject>("UI/UI_Produce_Mat"),contentMat.transform).GetComponent<UI_Produce_Mat>();
            ma.produce = this;
            ma.Init(matters[i].Item1, matters[i].Item2);
        }
    }

    public void Remove_ToMake()
    {
        toMake.gameObject.SetActive(false);
        for(int i=0;i<contentMat.transform.childCount;i++)
        {
            Destroy(contentMat.transform.GetChild(i).gameObject);
        }
        matters.Clear();
    } 

   
    //List -> item1 : 인벤 index, item2 : 필요 개수
    Dictionary<string, List<(int, int)>> inven_m = new();
    List<(int, int)> info_m;

    void OnProduce()
    {
        if (matters.Count == 0)
            return;
        
        if (CanProduce())
        {
            for (int i = 0; i < matters.Count; i++)
            {
                int count = matters[i].Item2;
                for (int j = 0; j < inven_m[matters[i].Item1].Count; j++)
                {
                    ItemSO item = Resources.Load<Item>($"Prefabs/Items/{matters[i].Item1}").itemSo;
                    Managers.Inven.SetSlot(item, Managers.Inven.inventoryUI.slotList[inven_m[matters[i].Item1][j].Item1].itemUI, Mathf.Clamp(info_m[j].Item2 - count,0, matters[i].Item2));
                    count -= info_m[j].Item2;
                }
            }
            Managers.Inven.AddOneItem(toMakeIDName);
        }
        else
            Debug.Log("재료가 부족합니다");
    }

    bool CanProduce()
    {
        inven_m.Clear();
        for(int i =0;i<matters.Count;i++)
        {
            bool correct = false;
            int _count = 0;
            for (int j = 0; j < Managers.Inven.inventorySlotInfo.Length - 1; j++)
            {
                StorageManager.SlotInfo slotInfo = Managers.Inven.inventoryUI.slotList[j].itemUI.slotInfo;
                if (slotInfo.itemInfo == null)
                    continue;
                if (matters[i].Item1 == slotInfo.itemInfo.idName)
                {
                    _count += slotInfo.count;
                    info_m = new()
                    {
                        (j, slotInfo.count)
                    };

                    if(_count >= matters[i].Item2)
                    {
                        inven_m.Add(matters[i].Item1, info_m);
                        correct = true;
                        break;
                    }
                }
            }
            if(!correct)
                return false;
        }

        return true;
    }
}
