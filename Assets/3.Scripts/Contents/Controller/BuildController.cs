using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class BuildController : MonoBehaviour
{
    public UI_Item itemUI;

    public SpriteRenderer buildItemIcon;


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

    public void MoveBuilder()
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
        if (Managers.Map.CheckCanBuild(new Vector3Int((int)(mousePosition.x), (int)(mousePosition.y), 0)))
        {
            if (!gameObject.activeSelf)
                return;
            MapManager.building.SetTile(new Vector3Int((int)(transform.position.x - tower.x), (int)(transform.position.y - tower.y), 0), itemUI.slotInfo.itemInfo.tile);
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
}
