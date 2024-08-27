using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Tilemaps;

public class TowerController : MonoBehaviour,IGetDamage,IDie,IInteractObject
{
    public static Action tutorial1Event;
    public static Action tutorial2Event;
    public static Action tutorial3Event;
    public Action forceInstallEvent;

    public GameObject canInteractSign { get; private set; }
    GameObject buildEffect;

    LayerMask buildLayer;
    LayerMask inviLayer;

    [HideInInspector]
    public Tilemap build;

    Stat stat;

    SpriteRenderer spriteRenderer;


    public AssetReferenceGameObject DieUIAsset;

    public void Init()
    {
        Managers.Game.tower = this;
        build = Util.FindChild(gameObject,"Building",true).GetComponent<Tilemap>();
        buildEffect = Util.FindChild(gameObject, "BuildEffect", true);
        MapManager.building = build;
        MapManager.tower = Util.FindChild(gameObject,"Tower",true).GetComponent<Tilemap>();
        TimeController.nightEvent += ForceInstall;

        canInteractSign = Util.FindChild(gameObject, "Sign");
        canInteractSign.SetActive(false);

        inviLayer.value = 8;
        buildLayer.value = 9;

        stat = GetComponent<Stat>();
        spriteRenderer = GetComponent<SpriteRenderer>();


    }

    public void Interact()
    {
        if (TimeController.timeType == TimeController.TimeType.Morning)
        {

            if (Managers.Game.isKeepingTower)
                return;

            if (!Managers.Game.completeTutorial)
                tutorial1Event.Invoke();

            Managers.Game.isKeepingTower = true;
            Managers.Inven.hotBarUI.CheckChoice();
            Managers.Inven.hotBarUI.towerSlot.ShowTowerIcon();
            Managers.Game.tower.transform.SetParent(Managers.Game.build.transform);
            Managers.Game.tower.transform.position = Managers.Game.build.transform.position;
        }
        else if(TimeController.timeType == TimeController.TimeType.Night)
        {
            TimeController.SetMorning();
            if (!Managers.Game.completeTutorial)
                tutorial3Event.Invoke();
        }
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
        Managers.Inven.CheckHotBarTowerSlot();
    }

    public void GetDamage(int damage)
    {
        if (PlayerController.isDie)
            return;
        stat.Hp -= damage;
        if (stat.Hp <= 0)
            Die();
    }

    public void BeforeInstallTower()
    {
        gameObject.SetActive(true);
        buildEffect.SetActive(false);
        spriteRenderer.color = new Color(1, 1, 1, 0.3f);
        gameObject.layer = inviLayer;
        GetComponent<BoxCollider2D>().enabled = false;
        MapManager.building.color = new Color(1, 1, 1, 0.3f);
        for (int i =0;i<MapManager.buildData.Count;i++)
        {
            GameObject go = MapManager.building.GetInstantiatedObject(MapManager.buildData[i]);
            go.GetComponent<Item_Buliding>().ChangeColorBeforeIntall();
            go.GetComponent<BoxCollider2D>().isTrigger = true;
        }
    }

    public void AfterInstallTower()
    {
        if (!Managers.Game.completeTutorial)
            tutorial2Event.Invoke();

        gameObject.SetActive(true);
        buildEffect.SetActive(true);
        spriteRenderer.color = new Color(1, 1, 1, 1);
        gameObject.layer = buildLayer;
        MapManager.building.color = new Color(1, 1, 1, 1f);
        GetComponent<BoxCollider2D>().enabled = true;
        for (int i = 0; i < MapManager.buildData.Count; i++)
        {
            GameObject go = MapManager.building.GetInstantiatedObject(MapManager.buildData[i]);
            go.GetComponent<Item_Buliding>().ChangeColorAfterIntall();
            go.GetComponent<BoxCollider2D>().isTrigger = false;
        }
    }

    public void Die()
    {
        DieUIAsset.InstantiateAsync().Completed += (go) =>
        {
            go.Result.GetComponent<Animator>().Play("GameOver");

            PlayerController.isDie = true;
            Camera.main.transform.parent = transform;
            Camera.main.transform.position = Camera.main.transform.parent.position + new Vector3(0, 0, -10);
        };
    }
}
