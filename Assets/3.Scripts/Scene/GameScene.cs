using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GameScene : BaseScene
{
    GameObject monsterSpawner;

    protected override bool Init()
    {
        if (base.Init() == false)
            return false;

        Managers.Game.isKeepingTower = false;
        PlayerController.isDie = false;
        Managers.Game.completeTutorial = true;
        //TimeController.timeSpeed = 1.2f;
        TimeController.timeSpeed = 20;
        Managers.UI.PopUIList.Clear();

        InstantiateOrLoad();
        SetActions();

        Managers.Game.grid.Init();
        Managers.Game.mouse.Init();
        Managers.Game.tower.Init();
        Managers.Inven.Init();
        Managers.Game.build.Init(); //¼öÁ¤ Áß
        Managers.Game.player.Init();
        Managers.Data.Init();

        return true;
    }

    private void OnDisable()
    {
        TimeController.morningEvent = null;
        TimeController.nightEvent = null;
        TimeController.timeEvent = null;
        TimeController.dayEvent = null;
    }

    void InstantiateOrLoad()
    {
        Instantiate(Resources.Load<GameObject>("UI/UI_Time"));
        Managers.Game.lightController = Instantiate(Resources.Load<GameObject>("Prefabs/Light")).GetComponent<LightController>();
        Managers.Game.grid = FindObjectOfType<MapManager>();
        Managers.Game.mouse = Instantiate(Resources.Load<GameObject>("Prefabs/MouseController").GetComponent<MouseController>());
        Managers.Game.build = Instantiate(Resources.Load<GameObject>("Prefabs/Builder")).GetComponent<BuildController>();
        Managers.Game.tower = Instantiate(Resources.Load<GameObject>("Prefabs/Tower")).GetComponent<TowerController>();
        Managers.Game.player = Instantiate(Resources.Load<GameObject>("Prefabs/Player")).GetComponent<PlayerController>();
        Instantiate(Resources.Load<GameObject>("Prefabs/NightMonsterSpawner"));
    }

    void SetActions()
    {
        Managers.Game.tower.SetAction();
        Managers.Game.build.SetAction();
    }
}
