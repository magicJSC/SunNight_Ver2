using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Long_Monster : MonsterController
{
    public GameObject go = null;

    void Atk()
    {
        if (go == null)
        {
            go = Resources.Load<GameObject>($"Prefabs/Monster/{Util.GetOriginalName(name)}_B");
        }
        GameObject g = Instantiate(go);
        Rigidbody2D r = g.GetComponent<Rigidbody2D>();
        g.transform.position = transform.position;
        r.velocity = (target.position - transform.position).normalized * 3;
        g.GetComponent<Bullet>().damage = _stat.Dmg;
    }
}
