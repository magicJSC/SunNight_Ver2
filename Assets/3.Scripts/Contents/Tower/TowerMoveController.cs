using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TowerMoveController : MonoBehaviour
{
    bool isMoving;
    bool canBuild;
    int lastMoveToggleFrame = -1;
    int lastBuildFrame = -1;

    Transform tower;
    Vector2 beforeTowerPos;

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.vKey.wasPressedThisFrame)
            ToggleTowerMove();

        if (isMoving && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            ConfirmTowerMove();
    }

    private void Start()
    {
        tower = Managers.Game.tower.transform;
    }

    public void TryMoveTower(InputAction.CallbackContext context)
    {
        if (context.performed)
            ToggleTowerMove();
    }

    void ToggleTowerMove()
    {
        if (FieldHUD.IsOpen || FieldLaboratory.PlayerInside) return;
        if (!Managers.Game.canMoveTower || FieldFoxtonBattle.Active != null) return;
        if (lastMoveToggleFrame == Time.frameCount)
            return;

        lastMoveToggleFrame = Time.frameCount;
        if (!isMoving)
            StartCoroutine(MoveTower());
        else
        {
            isMoving = false;
            Managers.Game.tower.SetAfterBuild();
            Managers.Map.SetCanBuildTile();
            MapManager.tower.gameObject.SetActive(false);
            Managers.Game.isCantPlay = false;
            Managers.Game.tower.transform.position = beforeTowerPos;
        }
    }

    public void TryBuildTowerAction(InputAction.CallbackContext context)
    {
        if (context.performed)
            ConfirmTowerMove();
    }

    void ConfirmTowerMove()
    {
        if (lastBuildFrame == Time.frameCount)
            return;

        lastBuildFrame = Time.frameCount;
        if (isMoving)
        {
            if (!canBuild)
                return;

            isMoving = false;
            Managers.Game.tower.SetAfterBuild();
            Managers.Map.SetCanBuildTile();
            MapManager.tower.gameObject.SetActive(false);
            Managers.Game.isCantPlay = false;
            Managers.Inven.CheckHotBarChoice();
        }
    }

    IEnumerator MoveTower()
    {
        isMoving = true;
        Managers.Game.tower.SetBeforeBuild();
        Managers.Game.isCantPlay = true;
        MapManager.tower.gameObject.SetActive(true);
        beforeTowerPos = tower.position;
        Vector2 curTowerPos = Vector2.positiveInfinity;
        canBuild = false;
        while (true)
        {
            yield return null;
            if (!isMoving) yield break;
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current != null ? (Vector3)Mouse.current.position.ReadValue() : Input.mousePosition);
            Vector2 towerPos = new Vector2(Mathf.Round(mousePos.x),Mathf.Round(mousePos.y));
            if (curTowerPos != towerPos)
            {
                tower.position = towerPos;
                curTowerPos = towerPos;
                if (Managers.Map.CheckCanBuild(new Vector3Int(-1, 0), new Vector3(2, 2),false))
                {
                    MapManager.tower.color = new Color(0.7f,1,0.6f, 0.4f);
                    canBuild = true;
                }
                else
                {
                    MapManager.tower.color = new Color(1, 0,0, 0.4f);
                    canBuild = false;
                }
            }
            if (!isMoving)
                yield break;
        }
    }
}
