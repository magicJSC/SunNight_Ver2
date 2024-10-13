using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterBullet : Bullet
{
    protected override void Hit(Collider2D col)
    {
        if (col.GetComponent<IMonster>() != null)
            return;

        if (col.TryGetComponent<IGetDamage>(out var getDamge))
        {
            getDamge.GetDamage(damage);
            Destroy(gameObject);
        }
    }
}
