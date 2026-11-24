using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannon : TurretController, IAttack, IRotate
{
    Transform face;
    float nextBossSceneShot;

    protected override void Init()
    {
        base.Init();
        face = Util.FindChild(gameObject, "Face", true).transform;
    }

    protected override void CheckTarget()
    {
        if(FieldSession.Active && UnityEngine.SceneManagement.SceneManager.GetActiveScene().name=="Boss1")
        {
            _target=null;
            foreach(var candidate in targets)
            {
                if(candidate==null || Vector2.Distance(transform.position,candidate.transform.position)>stat.range) continue;
                if(_target==null || Vector2.Distance(transform.position,candidate.transform.position)<Vector2.Distance(transform.position,_target.transform.position)) _target=candidate;
            }
            if(_target==null) return;
            Rotate();
            if(Time.time>=nextBossSceneShot)
            {nextBossSceneShot=Time.time+stat.attackCool;GetComponent<Animator>().Play("Work",0,0);}
            return;
        }
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
        if(FieldSession.Active && UnityEngine.SceneManagement.SceneManager.GetActiveScene().name=="Boss1" &&
            Vector2.Distance(transform.position,_target.transform.position)>stat.range) return;
        _target.GetComponent<IGetPlayerDamage>().GetDamage(FieldSession.Active ? (FieldSession.State.Has("U2") ? 7 : 5) : stat.Damage);
    }

    public override void AddTarget(Collider2D collision)
    {
        if (collision.GetComponent<MonsterController>() != null)
        {
            if(collision.GetComponent<IWalk>() != null)
                targets.Add(collision.gameObject);
        }
    }

    public override void RemoveTarget(Collider2D collision)
    {
        if (collision.GetComponent<MonsterController>() != null)
        {
            if (collision.GetComponent<IWalk>() != null)
                targets.Remove(collision.gameObject);
        }
    }
}
