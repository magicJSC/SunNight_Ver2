using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : ToolController
{
    [SerializeField]
    Vector2 size;

    

    void Attack()
    {
        Collider2D[] cols = Physics2D.OverlapBoxAll(transform.GetChild(0).position + (point - transform.position).normalized, size, angle);
        foreach (Collider2D col in cols)
        {
            if (col.TryGetComponent<IMonster>(out var monster))
            {
                monster.GetDamage(_damage);
            }
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.GetChild(0).position, size);
    }
}
