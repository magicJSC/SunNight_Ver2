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
    public string toMake_idName;

    [Header("UI")]
    Image toMake;

    GameObject back;
    GameObject content_Mat;
    GameObject content_Item;
    GameObject produce;
    GameObject hide;
    public GameObject explain;

    enum GameObjects
    {
        Background,
        Content_Mat,
        Content_Item,
        ToMake,
        Produce,
        Hide,
        Explain
    }

    public override void Init()
    {
        if (_init)
            return;

        _init = true;
        Bind<GameObject>(typeof(GameObjects));
        back = Get<GameObject>((int)GameObjects.Background);
        content_Mat = Get<GameObject>((int)GameObjects.Content_Mat);
        content_Item = Get<GameObject>((int)GameObjects.Content_Item);
        toMake = Get<GameObject>((int)GameObjects.ToMake).GetComponent<Image>();
        produce = Get<GameObject>((int)GameObjects.Produce);
        hide = Get<GameObject>((int)GameObjects.Hide);
        explain = Get<GameObject>((int)GameObjects.Explain);
        

        UI_EventHandler evt = back.GetComponent<UI_EventHandler>();
        evt._OnEnter += (PointerEventData p) => 
        {
            if(Managers.Game.mouse.CursorType != Define.CursorType.Drag)
            Managers.Game.mouse.CursorType = Define.CursorType.Normal;
        };
        evt._OnExit += (PointerEventData p) =>
        {
            if (Managers.Game.mouse.CursorType != Define.CursorType.Drag)
                Managers.Inven.Set_HotBar_Choice(); 
        };

        evt = produce.GetComponent<UI_EventHandler>();
        evt._OnClick += (PointerEventData p) => { OnProduce(); };

        evt = hide.GetComponent<UI_EventHandler>();
        evt._OnClick += (PointerEventData p) => { gameObject.SetActive(false); };

        for(int i = 0; i < content_Item.transform.childCount; i++)
        {
            UI_Produce_Item it = content_Item.transform.GetChild(i).GetComponent<UI_Produce_Item>();
            it.produce = this;
            it.Init();
        }
        explain.SetActive(false);
        Remove_ToMake();
    }

    private void OnEnable()
    {
        if(_init)
            Remove_ToMake();    
    }

    public void Set_ToMake(string itemName)
    {
        toMake.gameObject.SetActive(true);
        toMake.sprite = Resources.Load<Item>($"Prefabs/Items/{itemName}").itemIcon;
        toMake_idName = itemName;
            content_Mat.GetComponent<RectTransform>().offsetMax = new Vector2(content_Mat.GetComponent<RectTransform>().offsetMax.x + 100 * matters.Count -200, 0);
        for( int i = 0; i < matters.Count; i++)
        {
            UI_Produce_Mat ma = Instantiate(Resources.Load<GameObject>("UI/UI_Produce_Mat"),content_Mat.transform).GetComponent<UI_Produce_Mat>();
            ma.produce = this;
            ma.Init(matters[i].Item1, matters[i].Item2);
        }
    }

    public void Remove_ToMake()
    {
        toMake.gameObject.SetActive(false);
        for(int i=0;i<content_Mat.transform.childCount;i++)
        {
            Destroy(content_Mat.transform.GetChild(i).gameObject);
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
                    //제발 InvenManager에서 바로 사용 할 수 있게 해라 성찬아
                    Managers.Inven.inven.Set_Inven_Info(inven_m[matters[i].Item1][j].Item1, Mathf.Clamp(inven_m[matters[i].Item1][j].Item2 - count, 0, inven_m[matters[i].Item1][j].Item2), matters[i].Item1);
                }
            }
            Managers.Inven.AddItem(toMake_idName);
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
            for (int j = 0; j < Managers.Inven.inven_itemInfo.Length - 1; j++)
            {
                if (matters[i].Item1 == Managers.Inven.inven_itemInfo[j].idName)
                {
                    _count += Managers.Inven.inven_itemInfo[j].count;
                    info_m = new()
                    {
                        (j, Managers.Inven.inven_itemInfo[j].count)
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
