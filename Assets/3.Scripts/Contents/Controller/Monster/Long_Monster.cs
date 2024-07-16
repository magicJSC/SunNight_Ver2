using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Long_Monster : MonsterController
{
    GameObject go = null;
    public float bulletSpeed;

    void Atk()
    {
        if (go == null)
        {
            go = Resources.Load<GameObject>($"Prefabs/Monster/{Util.GetOriginalName(name)}_B");
        }
        GameObject g = Instantiate(go);
        Rigidbody2D r = g.GetComponent<Rigidbody2D>();
        g.transform.position = transform.position;
        r.velocity = (target.position - transform.position).normalized * bulletSpeed;
        g.GetComponent<Bullet>().damage = _stat.Damage;
    }
}
