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
            if(col.GetComponent<IGetDamage>() != null)
            {
                if(col.GetComponent<Stat>() != null)
                {
                    col.GetComponent<Stat>().Hp -= _stat.Damage;
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
                else if(col.GetComponent<PlayerStat>() != null)
                {
                    col.GetComponent<PlayerStat>().Hp -= _stat.Damage;
                    if (col.GetComponent<PlayerStat>().Hp <= 0)
                    {
                        Debug.Log("ÇÃ·¹ÀÌ¾î Á×À½");
                    }
                }
            }
            
        }
    }
}
