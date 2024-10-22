using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TowerMoveController : MonoBehaviour
{
    bool isMoving;
    bool canBuild;

    Transform tower;
    Vector2 beforeTowerPos;
    private void Start()
    {
        tower = Managers.Game.tower.transform;
    }

    public void TryMoveTower(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
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
    }

    public void TryBuildTowerAction(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            if (isMoving)
            {
                if (!canBuild)
                    return;

                isMoving = false;
                Managers.Game.tower.SetAfterBuild();
                Managers.Map.SetCanBuildTile();
                MapManager.tower.gameObject.SetActive(false);
                Managers.Game.isCantPlay = false;
            }
        }
    }

    IEnumerator MoveTower()
    {
        isMoving = true;
        Managers.Game.tower.SetBeforeBuild();
        Managers.Game.isCantPlay = true;
        MapManager.tower.gameObject.SetActive(true);
        beforeTowerPos = tower.position;
        Vector2 curTowerPos = tower.position;
        while (true)
        {
            yield return null;
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
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
