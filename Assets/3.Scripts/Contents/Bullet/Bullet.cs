using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [HideInInspector]
    public float damage;

    private void Start()
    {
        Destroy(gameObject,5);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.GetComponent<IGetDamage>() != null)
        {
            Hit(collision);
        }
    }

    void Hit(Collider2D col)
    {
        col.GetComponent<Stat>().Hp -= damage; 
        if(col.GetComponent<Stat>().Hp <= 0)
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
        Destroy(gameObject);
    }
}
