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
        if (col.GetComponent<IGetDamage>() != null)
        {
            if (col.TryGetComponent<IGetDamage>(out var getDamage))
            {
                getDamage.GetDamge(damage);
            }
        }
        Destroy(gameObject);
    }
}
