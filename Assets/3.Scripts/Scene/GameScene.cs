using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static InvenManager;

public class GameScene : BaseScene
{
    protected override bool Init()
    {
        if (base.Init() == false)
            return false;

        Managers.Game.isMovingTower = false;
        PlayerController.isDie = false;
        Managers.Game.completeTutorial = true;
        Managers.UI.PopUIList.Clear();

        for (int i = 0; i < 5; i++)
            Managers.Inven.hotBarSlotInfo[i] = new SlotInfo(0);
        for (int i = 0; i < 24; i++)
            Managers.Inven.inventorySlotInfo[i] = new SlotInfo(0);

        InstantiateOrLoad();
        SetActions();

        Managers.Game.timeController.timeSpeed = 2;
        Managers.Game.grid.Init();
        Managers.Game.mouse.Init();
        Managers.Game.tower.Init();
        Managers.Inven.Init();
        Managers.Game.build.Init(); //¼öÁ¤ Áß
        Managers.Game.player.Init();
        Managers.Game.lightController.Init();

        return true;
    }

    void InstantiateOrLoad()
    {
        Managers.Game.timeController = Instantiate(Resources.Load<GameObject>("UI/UI_Time")).GetComponent<TimeController>();
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
        Managers.Game.build.SetAction();
        Managers.Game.timeController.SetAction();
        Managers.Game.timeController.GetComponent<UI_Time>().SetAction();
        Managers.Game.lightController.SetAction();
    }
}
