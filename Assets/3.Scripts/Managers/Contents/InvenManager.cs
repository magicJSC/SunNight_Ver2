using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;
using UnityEngine.Tilemaps;

public class InvenManager : MonoBehaviour
{

    public ItemInfo[] hotBar_itemInfo = new ItemInfo[5];
    public ItemInfo[] inven_itemInfo = new ItemInfo[24];

    public class ItemInfo
    {
        public int id;
        public int count;
        public Sprite icon;
        public ItemType itemType;
        public KeyType keyType;
        public TileBase tile;

        public ItemInfo(int id, int count)
        {

            if (id == 0)
            {
                this.id = id;
                this.count = 0;
                keyType = KeyType.Empty;
                return;
            }
            Item i = Resources.Load<GameObject>($"Prefabs/Items/{id}").GetComponent<Item>(); //id에 따른 아이템 정보
            if (i == null)
                Debug.Log($"id가 {id}인 아이템은 없습니다");
            keyType = KeyType.Exist;
            this.id = id;
            itemType = i.itemType;
            icon = i.itemIcon;
            this.count = count;
            if (itemType == ItemType.Building)
                tile = i.tile;
        }
    }

    public void Init()
    {
        if (hotBar == null)
        {
            hotBar = FindAnyObjectByType<UI_HotBar>();
            if (hotBar == null)
            {
                hotBar = Instantiate(Resources.Load<GameObject>("UI/UI_HotBar/UI_HotBar")).GetComponent<UI_HotBar>();
            }
            hotBar.Init();
        }
        if (inven == null)
        {
            inven = FindAnyObjectByType<UI_Inven>();
            if (inven == null)
            {
                inven = Instantiate(Resources.Load<GameObject>("UI/UI_Inven/UI_Inven")).GetComponent<UI_Inven>();
            }
            inven.Init();
        }
    }

    #region 인벤토리
    public UI_HotBar hotBar;

    public int hotBar_choice = 0;
    public int HotBar_Choice { get { return hotBar_choice; } set { hotBar_choice = value; Set_HotBar_Choice(); } }

    //선택한 값에 따라 다르게 실행
    public void Set_HotBar_Choice()
    {
        //달라진 값을 가져오게 한다
        Managers.Game.build.SetInfo();
        if (Managers.Game.player.toolParent.transform.childCount != 0)
            Destroy(Managers.Game.player.toolParent.transform.GetChild(0).gameObject);
        switch (hotBar_itemInfo[hotBar_choice].itemType)
        {
            case ItemType.Building:
                Managers.Game.mouse.CursorType = CursorType.Builder;
                Managers.Game.build.ShowBuildSample();
                break;
            case ItemType.Tower:
                Managers.Game.mouse.CursorType = CursorType.Builder;
                Managers.Game.build.ShowTowerSample();
                break;
            case ItemType.Tool:
                Managers.Game.mouse.CursorType = CursorType.Normal;
                Instantiate(Resources.Load<GameObject>($"Prefabs/Items/{hotBar_itemInfo[hotBar_choice].id}"), Managers.Game.player.toolParent.transform);
                Managers.Game.build.HideSample();
                break;
            default:
                Managers.Game.mouse.CursorType = CursorType.Normal;
                Managers.Game.build.HideSample();
                break;
        }
    }



    public bool AddItem(int id, int count = 1)
    {
        Item item = Resources.Load<Item>($"Prefabs/Items/{id}");
        return Add_Item_Inven(item, count);
    }

    bool Add_Item_HotBar(Item item, int count = 1)
    {
        if (item.itemType != ItemType.Tool)   //재료 아이템일때
        {
            int empty = -1;
            for (int i = 0; i < hotBar_itemInfo.Length - 1; i++)
            {
                if (item.id == hotBar_itemInfo[i].id && hotBar_itemInfo[i].count < 99)
                {
                    hotBar.Set_HotBar_Info(i, item.id, hotBar_itemInfo[i].count + count);
                    return true;
                }
                else
                {
                    if (empty == -1 && KeyType.Empty == hotBar_itemInfo[i].keyType)
                        empty = i;
                }
            }
            //추가 하지 못했다면 비어있는 칸에 넣기
            if (empty != -1)
            {
                hotBar.Set_HotBar_Info(empty, item.id, hotBar_itemInfo[empty].count + count);
                return true;
            }

            return false;
        }
        else //도구 아이템일때
        {
            for (int i = 0; i < hotBar_itemInfo.Length - 1; i++)
            {
                if (KeyType.Empty == hotBar_itemInfo[i].keyType)
                {
                    hotBar.Set_HotBar_Info(i, item.id, 1);
                    return true;
                }
            }
            return false;
        }
    }

    bool Add_Item_Inven(Item item, int count = 1)
    {
        if (item.itemType != ItemType.Tool)   //재료 아이템일때
        {
            int empty = -1;
            for (int i = 0; i < inven_itemInfo.Length - 1; i++)
            {
                if (item.id == inven_itemInfo[i].id && inven_itemInfo[i].count < 99)
                {
                    inven.Set_Inven_Info(i, item.id, inven_itemInfo[i].count + count);
                    return true;
                }
                else
                {
                    if (empty == -1 && KeyType.Empty == inven_itemInfo[i].keyType)
                        empty = i;
                }
            }
            //추가 하지 못했다면 비어있는 칸에 넣기
            if (empty != -1)
            {
                inven.Set_Inven_Info(empty, item.id, inven_itemInfo[empty].count + count);
                return true;
            }

            //추가 하지 못했는데 비어있는 칸도 없을때
            return Add_Item_HotBar(item, count);
        }
        else //도구 아이템일때
        {
            for (int i = 0; i < inven_itemInfo.Length - 1; i++)
            {
                if (KeyType.Empty == inven_itemInfo[i].keyType)
                {
                    inven.Set_Inven_Info(i, item.id, 1);
                    return true;
                }
            }

            return Add_Item_HotBar(item, count);
        }
    }
    #endregion

    public UI_Inven inven;
    public (int index, InvenType invenType) changeSpot; //두번째에 받아오는 값들
}

