using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaboratoryScene : BaseScene
{
    protected override bool Init()
    {
        if (base.Init() == false)
            return false;

        Managers.Game.isKeepingTower = false;
        PlayerController.isDie = false;
        Managers.UI.PopUIList.Clear();

        InstantiateOrLoad();
        SetActions();

        Managers.Game.grid.Init();
        Managers.Game.mouse.Init();
        Managers.Data.Init();
        Managers.Inven.Init();
        Managers.Game.build.Init(); //¼öÁ¤ Áß
        Managers.Game.player.Init();
        Managers.Game.player.transform.position = new Vector2(6.7f,1.8f);

        return true;
    }

    void InstantiateOrLoad()
    {
        Managers.Game.mouse = Instantiate(Resources.Load<GameObject>("Prefabs/MouseController").GetComponent<MouseController>());
        Managers.Game.player = Instantiate(Resources.Load<GameObject>("Prefabs/Player")).GetComponent<PlayerController>();
        Managers.Game.grid = FindObjectOfType<MapManager>();
        Managers.Game.mouse = Instantiate(Resources.Load<GameObject>("Prefabs/MouseController").GetComponent<MouseController>());
        Managers.Game.build = Instantiate(Resources.Load<GameObject>("Prefabs/Builder")).GetComponent<BuildController>();
        Managers.Game.player.transform.position = new Vector3();
    }

    void SetActions()
    {
        Managers.Game.build.SetAction();
    }
}
