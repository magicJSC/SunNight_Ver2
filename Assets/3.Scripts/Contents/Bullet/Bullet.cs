using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Bullet : MonoBehaviour
{
    [HideInInspector]
    public int damage;

    private void Start()
    {
        Destroy(gameObject,5);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
         Hit(collision);
    }

    protected abstract void Hit(Collider2D col);
}
