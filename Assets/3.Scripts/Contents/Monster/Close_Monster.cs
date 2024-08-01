using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Close_Monster : MonsterController,IFly
{
    void Atk()
    {
        Collider2D[] cols = Physics2D.OverlapBoxAll(transform.position + (target.position - transform.position).normalized, new Vector2(2, 2), (target.position - transform.position).normalized.z);
        foreach (Collider2D col in cols)
        {
            if(col.transform == target)
            {
                target.TryGetComponent<IGetDamage>(out var getDamage);
                getDamage.GetDamage(stat.Damage);
                break;
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (target == null)
            return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position + (target.position - transform.position).normalized, new Vector2(2, 2));
    }
}
