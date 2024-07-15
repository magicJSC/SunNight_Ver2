using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterBullet : Bullet
{
    protected override void Hit(Collider2D col)
    {
        if (col.TryGetComponent<IBuilding>(out var build))
        {
            build.GetDamage(damage);
        }
        else if (col.TryGetComponent<IPlayer>(out var player))
        {
            player.GetDamage(damage);
        }
        Destroy(gameObject);
    }
}
