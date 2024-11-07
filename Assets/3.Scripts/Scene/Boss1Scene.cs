using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static InvenManager;

public class Boss1Scene : BaseScene
{
    protected override bool Init()
    {
        if (base.Init() == false)
            return false;

        InstantiateOrLoad();
        SetActions();


        Managers.Game.grid.Init();
        Managers.Game.mouse.Init();
        Managers.Game.tower.Init();
        Managers.Inven.Init();
        Managers.Game.build.Init(); //¼öÁ¤ Áß
        Managers.Game.player.Init();
        for (int i = 0; i < 5; i++)
            Managers.Inven.hotBarSlotInfo[i] = new SlotInfo(0);
        for (int i = 0; i < 24; i++)
            Managers.Inven.inventorySlotInfo[i] = new SlotInfo(0);

        Managers.Inven.hotBarSlotInfo[0] = new SlotInfo(10, "Fence");
        Managers.Inven.hotBarSlotInfo[1] = new SlotInfo(5, "Cannon");
        Managers.Inven.hotBarSlotInfo[2] = new SlotInfo(3, "LightningTower");
        Managers.Inven.hotBarSlotInfo[3] = new SlotInfo(1, "Bat");

        Managers.Game.player.transform.position = new Vector3(1, -9);
        Managers.Game.player.GetComponent<PlayerStat>().Hp = 100;
        Managers.Game.player.GetComponent<PlayerStat>().Hunger = 30;

        Managers.Game.tower.transform.position = new Vector3(1, -2);
        Managers.Game.player.transform.position = new Vector3(1.5f,-23);
        return true;
    }

    void InstantiateOrLoad()
    {
        Managers.Game.grid = FindObjectOfType<MapManager>();
        Managers.Game.mouse = Instantiate(Resources.Load<GameObject>("Prefabs/MouseController").GetComponent<MouseController>());
        Managers.Game.build = Instantiate(Resources.Load<GameObject>("Prefabs/Builder")).GetComponent<BuildController>();
        Managers.Game.tower = Instantiate(Resources.Load<GameObject>("Prefabs/Tower")).GetComponent<TowerController>();
        Managers.Game.player = Instantiate(Resources.Load<GameObject>("Prefabs/Player")).GetComponent<PlayerController>();
    }

    void SetActions()
    {
        Managers.Game.build.SetAction();
    }

}
