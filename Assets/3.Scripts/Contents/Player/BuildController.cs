using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class BuildController : MonoBehaviour
{
    public static Action tutorialEvent;

    [HideInInspector]
    public UI_Item itemUI;

    public SpriteRenderer buildItemIcon;
    public SpriteRenderer gridSign;

    public void Init()
    {
        gridSign = GetComponent<SpriteRenderer>();
        buildItemIcon = Util.FindChild(gameObject, "Sample").GetComponent<SpriteRenderer>();
        StartCoroutine(UpdateCor());
    }

    public void SetAction()
    {
        Managers.Inven.hotBarEvent += GetBuildItemInfo;

    }

    private void OnEnable()
    {
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
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition = new Vector2(Mathf.Round(mousePosition.x), Mathf.Round(mousePosition.y));
        transform.position = mousePosition;


        if (buildItemIcon == null)
            return;

        if (!Managers.Inven.choicingTower)
        {
            if (Managers.Map.CheckCanUseTile(new Vector3Int((int)(mousePosition.x), (int)(mousePosition.y), 0)))
            {
                buildItemIcon.color = new Color(1, 1, 1, 0.6f);
            }
            else
            {
                buildItemIcon.color = new Color(1, 0.5f, 0.5f, 0.6f);
            }
        }
        else
        {
            if (!Managers.Game.isKeepingTower)
                return;
            Vector2 tower = Managers.Game.tower.transform.position;
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (MapManager.cantBuild.HasTile(new Vector3Int((int)(tower.x + x), (int)(tower.y + y), 0)))
                    {
                        Managers.Game.canBuild = false;
                        Managers.Game.tower.transform.GetComponent<TowerController>().CantInstallTower();
                         return;
                    }
                }
            }
            for (int i = 0; i < MapManager.buildData.Count; i++)
            {
                if (MapManager.cantBuild.HasTile(MapManager.buildData[i] + new Vector3Int((int)tower.x, (int)tower.y, 0)))
                {
                    Managers.Game.canBuild = false;
                    Managers.Game.tower.transform.GetComponent<TowerController>().CantInstallTower();
                    return;
                }
            }
            Managers.Game.canBuild = true;
            Managers.Game.tower.GetComponent<TowerController>().BeforeInstallTower();
        }
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
        if (Managers.Map.CheckCanUseTile(new Vector3Int((int)(mousePosition.x), (int)(mousePosition.y), 0)))
        {
            if (!gameObject.activeSelf)
                return;

            if (!Managers.Game.completeTutorial)
                tutorialEvent.Invoke();
            if (itemUI == null)
                return;
            MapManager.building.SetTile(new Vector3Int((int)(transform.position.x - tower.x), (int)(transform.position.y - tower.y), 0), itemUI.slotInfo.itemInfo.buildTile); 
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
        else
        {
            Vector3Int pos = new Vector3Int((int)(mousePosition.x), (int)(mousePosition.y), 0);
            if (MapManager.building.HasTile(new Vector3Int(pos.x - (int)tower.x, pos.y - (int)tower.y)))
                Managers.Map.ShowBuildUI(new Vector3Int(pos.x - (int)tower.x, pos.y - (int)tower.y));
        }
    }

    public void HideBuildIcon()
    {
        buildItemIcon.gameObject.SetActive(false);
    }
}
