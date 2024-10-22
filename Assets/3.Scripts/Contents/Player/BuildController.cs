using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class BuildController : MonoBehaviour
{
    [HideInInspector]
    public UI_Item itemUI;

    public SpriteRenderer buildItemIcon;
    public SpriteRenderer buildTile;
    public SpriteRenderer gridSign;

    public void Init()
    {
        gridSign = GetComponent<SpriteRenderer>();
        buildItemIcon = Util.FindChild(gameObject, "Sample").GetComponent<SpriteRenderer>();
        buildTile = GetComponent<SpriteRenderer>();
        StartCoroutine(UpdateCor());
    }

    public void SetAction()
    {
        Managers.Inven.hotBarEvent += GetBuildItemInfo;
    }

    private void OnEnable()
    {
        Managers.Map.SetCanBuildTile();
        StartCoroutine(UpdateCor());
    }


    IEnumerator UpdateCor()
    {
        while (true)
        {
            MoveBuilder();
            yield return null;
        }
    }

    public void GetBuildItemInfo(UI_Item itemUI)
    {
        this.itemUI = itemUI;
        buildItemIcon.sprite = itemUI.slotInfo.itemInfo.itemIcon;
        buildItemIcon.gameObject.SetActive(true);
    }

    public void MoveBuilder()
    {
        if (Managers.Game.isCantPlay)
        {
            gameObject.SetActive(false);
            MapManager.canBuild.gameObject.SetActive(false);
            return;
        }
        if (MapManager.building == null)
            return;
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int gridPostion = MapManager.building.WorldToCell(mousePosition);
        transform.position = MapManager.building.CellToWorld(gridPostion) + new Vector3(0.5f,0.5f);

        if (buildItemIcon == null)
            return;
        if (!Managers.Game.tower.gameObject.activeSelf)
        {
            gameObject.SetActive(false);
            return;
        }
        if (Managers.Map.CheckCanUseTile(gridPostion))
        {
            buildTile.color = new Color(0.7f, 1, 0.6f, 0.4f);
        }
        else
        {
            buildTile.color = new Color(1, 0.4f, 0.4f, 0.4f);
        }
    }


    public void BuildItem()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int gridPostion = MapManager.building.WorldToCell(mousePosition);

        if (!Managers.Game.tower.gameObject.activeSelf)
            return;
        if (Managers.Map.CheckCanUseTile(gridPostion))
        {
            if (!gameObject.activeSelf)
                return;
            if (itemUI == null)
                return;
            MapManager.building.SetTile(gridPostion, itemUI.slotInfo.itemInfo.buildTile); 
            itemUI.slotInfo.count--;
            Managers.Map.SetCanBuildTile();
            if (itemUI.slotInfo.count <= 0)
            {
                itemUI.MakeEmptySlot();
                itemUI.SetEmptyItem();
                Managers.Game.mouse.CursorType = CursorType.Normal;
                Managers.Inven.CheckHotBarChoice();
            }
            else
                itemUI.SetInfo();
        }
        else
        {
            if (MapManager.building.HasTile(gridPostion))
                Managers.Map.ShowBuildUI(gridPostion);
        }
    }

    public void HideBuildIcon()
    {
        buildItemIcon.gameObject.SetActive(false);
    }
}
