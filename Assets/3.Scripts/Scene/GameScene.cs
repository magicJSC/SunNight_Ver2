using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameScene : BaseScene
{
    protected override bool Init()
    {
        if (base.Init() == false)
            return false;

        Managers.Game.isKeepingTower = false;
        PlayerController.isDie = false;
        Managers.Game.completeTutorial = true;
        TimeController.timeSpeed = 7;

        Instantiate(Resources.Load<GameObject>("UI/UI_Time"));
        Managers.Game.lightController = Instantiate(Resources.Load<GameObject>("Prefabs/Light")).GetComponent<LightController>();

        if (Managers.Game.grid == null)
        {
            Managers.Game.grid = FindObjectOfType<MapManager>();
        }
        Managers.Game.grid.Init();

        if (Managers.Game.mouse == null)
        {
            Managers.Game.mouse = Instantiate(Resources.Load<GameObject>("Prefabs/MouseController").GetComponent<MouseController>());
        }
        Managers.Game.mouse.Init();

        if (Managers.Game.build == null)
        {
            Managers.Game.build = Instantiate(Resources.Load<GameObject>("Prefabs/Builder")).GetComponent<BuildController>();
        }
        Managers.Game.build.Init();


        Managers.Game.tower = Instantiate(Resources.Load<GameObject>("Prefabs/Tower")).GetComponent<TowerController>();
        Managers.Game.tower.Init();

        Managers.Inven.Init();
        Managers.Game.player = Instantiate(Resources.Load<GameObject>("Prefabs/Player")).GetComponent<PlayerController>();
        Managers.Game.player.Init();
        Managers.Game.player.transform.position = new Vector3(-67.5f, 55.39f);
        Instantiate(Resources.Load<GameObject>("UI/UI_GameMenu"), Managers.Game.player.transform);

        return true;
    }

}
