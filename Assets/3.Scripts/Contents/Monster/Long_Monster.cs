using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class Long_Monster : MonsterController
{
    public AssetReferenceGameObject bulletAsset;

    GameObject bullet;

    public float bulletSpeed;

    public override void Init()
    {
        base.Init();
        bulletAsset.LoadAssetAsync().Completed += (obj)=>
        {
            bullet = obj.Result; 

        };
    }

    void Atk()
    {
        GameObject g = Instantiate(bullet,transform.position,Quaternion.identity);
        Rigidbody2D r = g.GetComponent<Rigidbody2D>();
        r.velocity = (target.position - transform.position).normalized * bulletSpeed;
        g.GetComponent<Bullet>().damage = stat.Damage;
    }
}
