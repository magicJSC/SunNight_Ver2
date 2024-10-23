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
    Animator anim;


    BoxCollider2D boxCollider;
    public void Init()
    {
        Managers.Game.tower = this;
        build = Util.FindChild(gameObject,"Building",true).GetComponent<Tilemap>();
        MapManager.building = build;
        MapManager.tower = Util.FindChild(gameObject,"Tower",true).GetComponent<Tilemap>();
        MapManager.tower.gameObject.SetActive(false);
        MapManager.canBuild = Util.FindChild(gameObject,"CanBuild",true).GetComponent<Tilemap>();
        stat = GetComponent<Stat>();
        boxCollider = GetComponent<BoxCollider2D>();
        anim = GetComponent<Animator>();
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

    public void SetAfterBuild()
    {
        boxCollider.enabled = true;
        anim.Play("Normal");
        for(int i=0;i<build.transform.childCount;i++)
        {
            if (!build.transform.GetChild(i).GetComponent<Item_Buliding>().ChangeAfterIntall())
            {
                i--;
            }
        }
    }

    public void SetBeforeBuild()
    {
        boxCollider.enabled = false;
        anim.Play("Move");
        for (int i = 0; i < build.transform.childCount; i++)
        {
            build.transform.GetChild(i).GetComponent<Item_Buliding>().ChangeBeforeIntall();
        }
    }

}
