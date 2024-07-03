using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TowerController : MonoBehaviour,IGetDamage
{
    bool canHold = false;
    LayerMask playerLayer;
    LayerMask buildLayer;
    LayerMask inviLayer;

    [HideInInspector]
    public Tilemap build;

    public void Init()
    {
        Managers.Game.tower = this;
        build = Util.FindChild(gameObject,"Building",true).GetComponent<Tilemap>();
        Managers.Game.grid.building = build;
        TimeController.nightEvent += ForceInstall;
        Camera.main.GetComponent<CameraController>().target = transform;
        playerLayer = 6;
        buildLayer.value = 9;
        inviLayer.value = 8;
    }

    private void Update()
    {
        if (TimeController.timeType == TimeController.TimeType.Night)
            return;

        if (Input.GetKeyDown(KeyCode.F) && canHold)
        {
            Managers.Inven.hotBarUI.GetTower();
        }
    }

    public void ChangeInvisable()
    {
        gameObject.layer = inviLayer;
        for (int i = 0; i < build.transform.childCount; i++)
        {
            build.transform.GetChild(i).gameObject.layer = inviLayer;
        }
    }

    public void ChangeVisable()
    {
        gameObject.layer = buildLayer;
        for (int i = 0; i < build.transform.childCount; i++)
        {
            build.transform.GetChild(i).gameObject.layer = buildLayer;
        }
    }

    void ForceInstall()
    {
        if (!Managers.Game.isKeepingTower)
            return;

        Managers.Game.build.BuildTower();
        Vector2 playerPos = Managers.Game.player.transform.position;
        transform.position = new Vector2(Mathf.Round(playerPos.x), Mathf.Round(playerPos.y));
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == playerLayer)
        {
            canHold = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == playerLayer)
        {
            canHold = false;
        }
    }
}
