using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrongLaser : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.TryGetComponent<Stat>(out var stat))
        {
            stat.GetDamage(9999);
        }
    }
}
