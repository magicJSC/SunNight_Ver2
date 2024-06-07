using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [HideInInspector]
    public float damage;
    public LayerMask playerLayer;
    public LayerMask budildLayer;
    private void Start()
    {
        Destroy(gameObject,5);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.layer == Mathf.Log(playerLayer.value,2) || collision.gameObject.layer == Mathf.Log(budildLayer.value, 2))
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
