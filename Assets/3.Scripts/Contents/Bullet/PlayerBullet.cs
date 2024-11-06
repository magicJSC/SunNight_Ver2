using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBullet : Bullet
{
    protected override void Hit(Collider2D col)
    {
        if (col.GetComponent<IGetMonsterDamage>() != null)
            return;

       if(col.TryGetComponent<IGetPlayerDamage>(out var monster))
        {
            monster.GetDamage(damage);
        }
        Destroy(gameObject);
    }
}
