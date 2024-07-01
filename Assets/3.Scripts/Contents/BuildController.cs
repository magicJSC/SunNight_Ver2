using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class BuildController : MonoBehaviour
{
    public UI_Item itemUI;

    SpriteRenderer buildItemIcon;


    public void Init()
    {
        Managers.Game.build = this;
        buildItemIcon = Util.FindChild(gameObject, "Sample").GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        MoveBuilder();
    }

    public void GetBuildItemInfo(UI_Item uI_Item)
    {
        itemUI = uI_Item;
        buildItemIcon.sprite = itemUI.slotInfo.itemInfo.itemIcon;
    }

    void MoveBuilder()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition = new Vector2(Mathf.Round(mousePosition.x), Mathf.Round(mousePosition.y));
        transform.position = mousePosition;
    }

    public void BuildTower()
    {
        Managers.Game.tower.transform.parent = null;
        Managers.Game.isKeepingTower = false;
        Managers.Inven.hotBarUI.CheckChoice();
    }

    public void BuildItem()
    {
        Vector2 tower = Managers.Game.tower.transform.position; //기지 위치 받아오기
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition = new Vector2(Mathf.Round(mousePosition.x), Mathf.Round(mousePosition.y));
        if (!Managers.Game.grid.CheckCanBuild(new Vector3Int((int)(mousePosition.x), (int)(mousePosition.y), 0)))
            ShowBuildUI(new Vector3Int((int)(mousePosition.x), (int)(mousePosition.y), 0));
        else
        {
            if (!gameObject.activeSelf)
                return;
            Managers.Game.tower.build.SetTile(new Vector3Int((int)(transform.position.x - tower.x), (int)(transform.position.y - tower.y), 0), itemUI.slotInfo.tile);
            itemUI.slotInfo.count--;
            if (itemUI.slotInfo.count <= 0)
            {
                itemUI.MakeEmptySlot();
                itemUI.SetEmptyItem();
                Managers.Game.mouse.CursorType = CursorType.Normal;
            }
            else
                itemUI.SetInfo();
        }
    }

    public void ShowBuildIcon()
    {
        buildItemIcon.gameObject.SetActive(true);
    }

    public void HideBuildIcon()
    {
        buildItemIcon.gameObject.SetActive(false);
    }

    #region 건축

    //void DeleteTile()
    //{
    //    if (Managers.Game.mouse.CursorType != CursorType.Builder)
    //        return;
    //    //타워를 소장 하고 있을때
    //    if (Managers.Inven.hotBar_itemInfo[Managers.Inven.hotBar_itemInfo.Length - 1].keyType == Define.KeyType.Exist)
    //        return;

    //    Vector2 tower = Managers.Game.tower.transform.position; //기지 위치 받아오기
    //    GameObject go = Managers.Game.tower.build.GetInstantiatedObject(new Vector3Int((int)(transform.position.x - tower.x), (int)(transform.position.y - tower.y), 0));
    //    if (go != null)
    //    {
    //        Managers.Inven.AddItem(go.GetComponent<Item>().idName);
    //        Managers.Inven.Set_HotBar_Choice();
    //        go.GetComponent<Item_Buliding>().DeleteBuilding();
    //    }
    //}

    //강화 할수 있는 UI 생성
    void ShowBuildUI(Vector3Int pos)
    {
        if (Managers.Game.mouse.CursorType == CursorType.Battle)
            return;
        Vector2 towerPos = Managers.Game.tower.transform.position;
        if (pos == new Vector3Int((int)towerPos.x, (int)towerPos.y))
            return;
        GameObject go = Managers.Game.grid.building.GetInstantiatedObject(new Vector3Int(pos.x - (int)towerPos.x, pos.y - (int)towerPos.y));
        go.GetComponent<Item_Buliding>().buildUI.SetActive(true);
    }

    //public void ShowBuildSample()
    //{
    //    //아이템이 설치 아이템일때
    //    if (Managers.Inven.hotBar_itemInfo[Managers.Inven.HotBar_Choice].keyType == Define.KeyType.Exist)
    //    {
    //        sample.SetActive(true);
    //        sample.GetComponent<SpriteRenderer>().sprite = Managers.Inven.hotBar_itemInfo[Managers.Inven.HotBar_Choice].itemInfo.itemIcon;
    //    }

    //    //건축 모드가 아닐때 소장하고 있을때 
    //    if (Managers.Game.mouse.CursorType != Define.CursorType.Builder && Managers.Inven.hotBar_itemInfo[Managers.Inven.hotBar_itemInfo.Length - 1].keyType == KeyType.Exist)
    //        Managers.Game.tower.gameObject.SetActive(false);

    //    //건축 모드일때 기지를 소장하고 있고 다른 선택을 하고 있을때
    //    if (Managers.Game.mouse.CursorType == Define.CursorType.Builder && Managers.Inven.hotBar_itemInfo[Managers.Inven.hotBar_itemInfo.Length - 1].keyType == KeyType.Exist && Managers.Inven.HotBar_Choice != Managers.Inven.hotBar_itemInfo.Length - 1)
    //        Managers.Game.tower.gameObject.SetActive(false);
    //} 



    //public void BuildTower(bool force = false)
    //{
    //    //강제 설치
    //    if (force)
    //        Managers.Game.tower.transform.position = Managers.Game.player.transform.position;

    //    Managers.Inven.hotBar_itemInfo[Managers.Inven.hotBar_itemInfo.Length - 1].keyType = Define.KeyType.Empty;
    //    Managers.Game.tower.gameObject.SetActive(true);
    //    Managers.Game.tower.GetComponent<SpriteRenderer>().color = new Color(1,1,1,1);
    //    Managers.Game.tower.build.color = new Color(1,1,1,1);
    //    Managers.Inven.Set_HotBar_Choice();
    //    Managers.Game.tower.ChangeVisable();
    //}

    //public void ShowTowerSample()
    //{
    //     //기지를 선택할때 기지가 존재한다면
    //    if (Managers.Inven.hotBar_itemInfo[Managers.Inven.hotBar_itemInfo.Length - 1].keyType == Define.KeyType.Exist)
    //    {
    //        Managers.Game.tower.gameObject.SetActive(true);
    //        Managers.Game.tower.GetComponent<SpriteRenderer>().color = new Color32(225, 225, 225, 120);
    //        Managers.Game.tower.build.color = new Color32(225, 225, 225, 120);
    //        Managers.Game.tower.ChangeInvisable();
    //    }
    //    sample.SetActive(false);
    //}
    #endregion


}
