using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Tilemaps;

public class TowerController : MonoBehaviour,IGetDamage,IDie
{
    public Action forceInstallEvent;

    [HideInInspector]
    public Tilemap build;

    Stat stat;

    public AssetReferenceGameObject DieUIAsset;

    public void Init()
    {
        Managers.Game.tower = this;
        build = Util.FindChild(gameObject,"Building",true).GetComponent<Tilemap>();
        MapManager.building = build;
        MapManager.tower = Util.FindChild(gameObject,"Tower",true).GetComponent<Tilemap>();
        stat = GetComponent<Stat>();
    }

    public void GetDamage(int damage)
    {
        if (PlayerController.isDie)
            return;
        stat.Hp -= damage;
        if (stat.Hp <= 0)
            Die();
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
