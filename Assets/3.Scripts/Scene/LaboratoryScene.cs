using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static InvenManager;

public class LaboratoryScene : BaseScene
{
    protected override bool Init()
    {
        if (base.Init() == false)
            return false;

        Managers.Game.isMovingTower = false;
        PlayerController.isDie = false;
        Managers.UI.PopUIList.Clear();

        InstantiateOrLoad();
        SetActions();

        for (int i = 0; i < 5; i++)
            Managers.Inven.hotBarSlotInfo[i] = new SlotInfo(0);
        for (int i = 0; i < 24; i++)
            Managers.Inven.inventorySlotInfo[i] = new SlotInfo(0);

        Managers.Game.grid.Init();
        Managers.Game.mouse.Init();
        
        Managers.Inven.Init();
        Managers.Game.build.Init(); //¼öÁ¤ Áß
        Managers.Game.player.Init();
        Managers.Game.player.transform.position = new Vector2(6.7f,1.8f);
        Managers.Game.player.GetComponent<PlayerStat>().Hp = 100;
        Managers.Game.player.GetComponent<PlayerStat>().Hunger = 30;


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
