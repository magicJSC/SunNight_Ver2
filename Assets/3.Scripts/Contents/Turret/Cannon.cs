using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannon : TurretController, IAttack, IRotate
{
    Transform face;

    protected override void Init()
    {
        base.Init();
        face = Util.FindChild(gameObject, "Face", true).transform;
    }

    protected override void CheckTarget()
    {
        base.CheckTarget();

        if (_target != null)
         Rotate();
    }


    public void Rotate()
    {
        Vector3 dir = (_target.transform.position - transform.position).normalized;
        float rot = Mathf.Atan2(-dir.y, -dir.x) * Mathf.Rad2Deg;
        face.rotation = Quaternion.Euler(0, 0, rot + 180);
    }

    public void Attack()
    {
        if (_target == null)
            return;
        _target.GetComponent<IMonster>().GetDamage(stat.Damage);
    }

    public override void AddTarget(Collider2D collision)
    {
        if (collision.GetComponent<MonsterController>() != null)
        {
            if(collision.GetComponent<IFly>() != null)
                targets.Add(collision.gameObject);
        }
    }

    public override void RemoveTarget(Collider2D collision)
    {
        if (collision.GetComponent<MonsterController>() != null)
        {
            if (collision.GetComponent<IFly>() != null)
                targets.Remove(collision.gameObject);
        }
    }
}
