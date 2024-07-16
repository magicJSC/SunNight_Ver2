using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TowerController : MonoBehaviour,IGetDamage,ICaninteract,IDie
{
    public Action forceInstallEvent;
    LayerMask buildLayer;
    LayerMask inviLayer;

    [HideInInspector]
    public Tilemap build;

    Stat stat;

    SpriteRenderer spriteRenderer;

    public GameObject canInteractSign { get; set; }

    public void Init()
    {
        Managers.Game.tower = this;
        build = Util.FindChild(gameObject,"Building",true).GetComponent<Tilemap>();
        MapManager.building = build;
        MapManager.tower = Util.FindChild(gameObject,"Tower",true).GetComponent<Tilemap>();
        TimeController.nightEvent += ForceInstall;

        canInteractSign = Util.FindChild(gameObject, "Sign");
        canInteractSign.SetActive(false);

        Camera.main.GetComponent<CameraController>().target = transform;
        inviLayer.value = 8;
        buildLayer.value = 9;

        stat = GetComponent<Stat>();
        spriteRenderer = GetComponent<SpriteRenderer>();
       
    }

    public void Interact()
    {
        if (Managers.Game.isKeepingTower || TimeController.timeType == TimeController.TimeType.Night)
            return;

        Managers.Game.isKeepingTower = true;
        Managers.Inven.hotBarUI.CheckChoice();
        Managers.Inven.hotBarUI.towerSlot.ShowTowerIcon();
        Managers.Game.tower.transform.SetParent(Managers.Game.build.transform);
        Managers.Game.tower.transform.position = Managers.Game.build.transform.position;
    }

    void ForceInstall()
    {
        if (!Managers.Game.isKeepingTower)
            return;

        forceInstallEvent?.Invoke();
        Managers.Game.build.BuildTower();
        Vector2 playerPos = Managers.Game.player.transform.position;
        transform.position = new Vector2(Mathf.Round(playerPos.x), Mathf.Round(playerPos.y));
        AfterInstallTower();
    }

    public void GetDamage(float damage)
    {
        stat.Hp -= damage;
        if (stat.Hp <= 0)
            Debug.Log("게임 오버");
    }

    public void BeforeInstallTower()
    {
        spriteRenderer.color = new Color(1, 1, 1, 0.3f);
        gameObject.layer = inviLayer;
        GetComponent<BoxCollider2D>().isTrigger = true;
        for(int i =0;i<MapManager.buildData.Count;i++)
        {
            GameObject go = MapManager.building.GetInstantiatedObject(MapManager.buildData[i]);
            go.GetComponent<Item_Buliding>().ChangeColorBeforeIntall();
            go.GetComponent<BoxCollider2D>().isTrigger = true;
        }
    }

    public void AfterInstallTower()
    {
        spriteRenderer.color = new Color(1, 1, 1, 1);
        gameObject.layer = buildLayer;
        GetComponent<BoxCollider2D>().isTrigger = false;
        for (int i = 0; i < MapManager.buildData.Count; i++)
        {
            GameObject go = MapManager.building.GetInstantiatedObject(MapManager.buildData[i]);
            go.GetComponent<Item_Buliding>().ChangeColorAfterIntall();
            go.GetComponent<BoxCollider2D>().isTrigger = false;
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<PlayerController>(out var player))
        {
            EnterPlayer(player);
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<PlayerController>(out var player))
        {
            ExitPlayer(player);
        }
    }

    public void EnterPlayer(PlayerController player)
    {
        player.interactObjectList.Add(gameObject);
        player.SetInteractObj();
    }

    public void ExitPlayer(PlayerController player)
    {
        player.interactObjectList.Remove(gameObject);
        canInteractSign.SetActive(false);
        player.SetInteractObj();
    }

    public void Die()
    {
        throw new NotImplementedException();
    }
}
