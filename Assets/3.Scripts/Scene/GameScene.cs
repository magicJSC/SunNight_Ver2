using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScene : BaseScene
{
    protected override bool Init()
    {
        if (base.Init() == false)
            return false;

        Managers.Game.isKeepingTower = false;

        Instantiate(Resources.Load<GameObject>("UI/UI_Time"));
        Managers.Game.lightController = Instantiate(Resources.Load<GameObject>("Prefabs/Light")).GetComponent<LightController>();
        Managers.Game.Init();
        Managers.Inven.Init();
        Managers.Game.player = Instantiate(Resources.Load<GameObject>("Prefabs/Player")).GetComponent<PlayerController>();
        Managers.Game.player.Init();
        Instantiate(Resources.Load<GameObject>("UI/UI_GameMenu"), Managers.Game.player.transform);

        return true;
    }

}
