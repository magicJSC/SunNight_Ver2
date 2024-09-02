using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Close_Monster : MonsterController,IWalk
{
    void Atk()
    {
        if (target == null)
            return;
        Collider2D[] cols = Physics2D.OverlapBoxAll(transform.position + (target.position - transform.position) / 2, new Vector2(stat.attackRange, stat.attackRange), (target.position - transform.position).normalized.z);
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
        Gizmos.DrawWireCube(transform.position + (target.position - transform.position) / 2, new Vector2(stat.attackRange, stat.attackRange));
    }
}
