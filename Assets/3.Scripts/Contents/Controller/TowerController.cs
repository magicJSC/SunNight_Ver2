using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TowerController : MonoBehaviour
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
        Camera.main.GetComponent<CameraController>().target = transform;
        playerLayer = 6;
        buildLayer.value = 9;
        inviLayer.value = 8;
    }

    private void Update()
    {
        if (Managers.Game.timeType == Define.TimeType.Night)
            return;

        if (Input.GetKeyDown(KeyCode.F) && canHold)
        {
            Managers.Inven.hotBar.GetTower();
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

    private void OnTriggerStay2D(Collider2D collision)
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
