using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Close_Monster : MonsterController
{
    void Atk()
    {
        Collider2D col = Physics2D.OverlapBox(transform.position + (target.position - transform.position).normalized / 2, new Vector2(1, 1), (target.position - transform.position).normalized.z);
        if (col != null)
        {
            if(col.TryGetComponent<IGetDamage>(out var getDamage))
            {
                getDamage.GetDamge(_stat.Damage);
            }
        }
    }
}
