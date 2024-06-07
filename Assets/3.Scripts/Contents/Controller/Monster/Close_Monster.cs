using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Close_Monster : MonsterController
{
    void Atk()
    {
        Collider2D col = Physics2D.OverlapBox(transform.position + (target.position - transform.position).normalized / 2, new Vector2(1, 1), (target.position - transform.position).normalized.z, buildLayer | playerLayer);
        if (col != null)
        {
            col.GetComponent<Stat>().Hp -= _stat.Dmg;
            if (col.GetComponent<Stat>().Hp <= 0)
            {
                if (col.GetComponent<Item>())
                {
                    col.GetComponent<Item_Buliding>().DeleteBuilding();
                }
                else if (col.GetComponent<TowerController>())
                {
                    Debug.Log("±âÁö ÆÄ±«");
                }
            }
        }
    }
}
