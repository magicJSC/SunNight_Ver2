using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static Define;

public class BuildController : MonoBehaviour
{
    [HideInInspector]
    public UI_Item itemUI;

    public SpriteRenderer buildItemIcon;
    public SpriteRenderer buildTile;
    public SpriteRenderer gridSign;
    bool initialized;

    public void Init()
    {
        gridSign = GetComponent<SpriteRenderer>();
        buildItemIcon = Util.FindChild(gameObject, "Sample").GetComponent<SpriteRenderer>();
        buildTile = GetComponent<SpriteRenderer>();
        Managers.Map.SetCanBuildTile();
        initialized = true;
        if (isActiveAndEnabled)
        {
            StopAllCoroutines();
            StartCoroutine(UpdateCor());
        }
    }

    public void SetAction()
    {
        Managers.Inven.hotBarEvent += GetBuildItemInfo;
    }

    private void OnEnable()
    {
        // Instantiate invokes OnEnable before the scene has initialized the new map and tower.
        if (!initialized) return;
        if (Managers.Game.tower != null && MapManager.building != null && MapManager.tower != null && MapManager.canBuild != null && MapManager.cantBuild != null)
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
        if (MapManager.canBuild == null || MapManager.building == null || Managers.Game.tower == null || Camera.main == null)
            return;
        if (Managers.Game.isCantPlay)
        {
            gameObject.SetActive(false);
            MapManager.canBuild.gameObject.SetActive(false);
            return;
        }
        if (MapManager.building == null)
            return;
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint((Mouse.current != null ? (Vector3)Mouse.current.position.ReadValue() : Input.mousePosition));
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
        if (Camera.main == null) return;
        BuildAtWorldPoint(Camera.main.ScreenToWorldPoint((Mouse.current != null ? (Vector3)Mouse.current.position.ReadValue() : Input.mousePosition)));
    }

    // Shared placement command: pointer input and automated gameplay checks use identical rules.
    public void BuildAtWorldPoint(Vector3 worldPoint)
    {
        if (FieldHUD.IsOpen || FieldFoxtonBattle.Active != null || Managers.Game.isCantPlay || Managers.Game.isHandleUI) return;
        Vector3Int gridPostion = MapManager.building.WorldToCell(worldPoint);

        if (!Managers.Game.tower.gameObject.activeSelf)
            return;
        if (Managers.Map.CheckCanUseTile(gridPostion))
        {
            if (!gameObject.activeSelf)
                return;
            if (itemUI == null || itemUI.slotInfo.itemInfo == null || itemUI.slotInfo.count <= 0)
                return;
            if (FieldSession.Active && itemUI.slotInfo.itemInfo.idName.StartsWith("Cannon") && ExplorationEffects.CannonCount() >= FieldSession.CannonLimit)
            { FieldHUD.Notify("대포 슬롯이 가득 찼습니다."); return; }
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
