using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class Gun : ToolController
{
    [SerializeField]
    float bulletSpeed;

    [SerializeField]
    AssetReferenceGameObject asset;

    GameObject bullet;

    void Attack()
    {
        angle = Mathf.Atan2(point.y - transform.position.y, point.x - transform.position.x) * Mathf.Rad2Deg;
        asset.InstantiateAsync(transform.position, Quaternion.Euler(0,0,angle)).Completed += (obj) =>
        {
            bullet = obj.Result;
            bullet.GetComponent<Rigidbody2D>().velocity = (point - transform.position).normalized * bulletSpeed;
            bullet.GetComponent<PlayerBullet>().damage = _damage;
        };
    }
}
