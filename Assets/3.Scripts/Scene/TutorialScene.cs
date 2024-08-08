using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialScene : BaseScene
{
    protected override bool Init()
    {
        if (base.Init() == false)
            return false;
        if (Managers.Game.grid == null)
        {
            Managers.Game.grid = FindAnyObjectByType<MapManager>();
        }
        Managers.Game.lightController = Instantiate(Resources.Load<GameObject>("Prefabs/Light")).GetComponent<LightController>();
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

        return true;
    }
}
