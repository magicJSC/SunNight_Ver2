using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : ToolController
{
    [SerializeField]
    float bulletSpeed;

    [SerializeField]
    GameObject bullet;

   void Attack()
    {
        GameObject b = Instantiate(bullet);
        b.transform.position = transform.position;
        angle = Mathf.Atan2(point.y - transform.position.y, point.x - transform.position.x) * Mathf.Rad2Deg;
        b.transform.rotation = Quaternion.AngleAxis(angle, b.transform.forward);
        b.GetComponent<Rigidbody2D>().velocity = (point - transform.position).normalized * bulletSpeed;
        b.GetComponent<Bullet>().damage = _damage;
    }
}
