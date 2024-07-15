using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBullet : Bullet
{
    protected override void Hit(Collider2D col)
    {
        if (col.GetComponent<IPlayer>() != null)
            return;

       if(col.TryGetComponent<IMonster>(out var monster))
        {
            monster.GetDamage(damage);
        }
        Destroy(gameObject);
    }
}
