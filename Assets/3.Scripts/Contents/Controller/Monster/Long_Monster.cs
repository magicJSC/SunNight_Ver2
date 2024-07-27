using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class Long_Monster : MonsterController
{
    public AssetReferenceGameObject bulletAsset;

    public float bulletSpeed;

    void Atk()
    {
        GameObject g = bulletAsset.InstantiateAsync().Result;
        Rigidbody2D r = g.GetComponent<Rigidbody2D>();
        g.transform.position = transform.position;
        r.velocity = (target.position - transform.position).normalized * bulletSpeed;
        g.GetComponent<Bullet>().damage = stat.Damage;
    }
}
