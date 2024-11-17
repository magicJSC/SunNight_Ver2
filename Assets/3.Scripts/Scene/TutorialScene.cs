using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static InvenManager;

public class TutorialScene : BaseScene
{
    [SerializeField]
    AudioClip mainSound;
    protected override bool Init()
    {
        if (base.Init() == false)
            return false;

        Managers.Sound.Play(Define.Sound.Bgm, mainSound);

        InstantiateOrLoad();
        SetActions();

        Managers.Game.timeController.day = 0;

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

        Managers.Game.player.transform.position = new Vector2(-53,22);
        Managers.Game.timeController.gameObject.SetActive(false);
        Managers.Game.player.GetComponent<PlayerStat>().Hp = 100;
        Managers.Game.player.GetComponent<PlayerStat>().Hunger = 30;

        Managers.Game.timeController.timeSpeed = 2f;

        Managers.Game.tower.transform.position = new Vector3(-37, 37);
        return true;
    }

    void InstantiateOrLoad()
    {
        Managers.Game.timeController = Instantiate(Resources.Load<GameObject>("UI/UI_Time")).GetComponent<TimeController>();
        Managers.Game.timeController.GetComponent<UI_Time>().Init();
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
