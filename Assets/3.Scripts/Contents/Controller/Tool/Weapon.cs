using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : ToolController
{
    [SerializeField]
    Vector2 size;
    [SerializeField]
    int _damage;
    LayerMask monsterLayer;

    protected override void Init()
    {
        base.Init();
        monsterLayer = 7;
    }

    protected override void Ready()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            anim.Play("Attack");
            isWorking = true;
        }
    }

    void EndAtk()
    {
        isWorking = false;
    }

    void Check()
    {
        Collider2D[] cols = Physics2D.OverlapBoxAll(transform.GetChild(0).position + (point - transform.position).normalized, size, angle);
        foreach (Collider2D col in cols)
        {
            if (col.gameObject.layer == monsterLayer)
            {
                MonsterStat stat = col.GetComponent<MonsterStat>();
                stat.Hp = Util.GetTotalHp(stat.Hp, stat.Def, _damage);
                if (stat.Hp <= 0)
                {
                    Destroy(col.gameObject);
                }
            }
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.GetChild(0).position, size);
    }
}
