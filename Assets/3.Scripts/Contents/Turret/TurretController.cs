using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRotate
{
    public void Rotate();
}

public interface IAttack
{
    public void Attack();
}


public class TurretController : BaseController
{
    protected Action workEvent;

    protected BuildStat stat;
    Animator anim;

    float termBeforeWork = 1;

    public List<GameObject> targets = new List<GameObject>();

    protected GameObject _target;

    protected bool isWorking = false;
    bool packed;
    float nextAttack;
    float cooldownWhenPacked;

    public void PauseForTransport()
    {
        if (packed) return;
        packed = true;
        cooldownWhenPacked = Mathf.Max(0, nextAttack - Time.time);
        StopAllCoroutines();
        anim.Rebind();
        _target = null;
        isWorking = false;
    }

    public void ResumeAfterTransport()
    {
        if (!packed) return;
        packed = false;
        nextAttack = Time.time + Mathf.Max(termBeforeWork, cooldownWhenPacked);
        if (isActiveAndEnabled) StartCoroutine(UpdateCor());
    }


    protected override void Init()
    {
        anim = GetComponent<Animator>();
        stat = GetComponent<BuildStat>();
        if (stat == null)
            Debug.Log($"{name}포탑에 TurretStat이 존재하지 않습니다");

        
    }

    private void OnEnable()
    {
        if (!packed) StartCoroutine(UpdateCor());
    }

    IEnumerator UpdateCor()
    {
        while (true)
        {
            CheckTarget();
            yield return null;
        }
    }

    protected virtual void CheckTarget()
    {
        if (isWorking)
            return;

        if (_target == null)
        {
            SetTarget();
            return;
        }
        else if (Vector2.Distance(_target.transform.position, transform.position) > stat.range)
        {
            SetTarget();
            return;
        }
    }



    protected IEnumerator Work()
    {
        yield return new WaitForSeconds(Mathf.Max(termBeforeWork, nextAttack - Time.time));
        while (true)
        {
            if (_target == null)
                yield break;
            isWorking = true;
            nextAttack = Time.time + stat.attackCool;
            anim.Play("Work");
            yield return new WaitForSeconds(stat.attackCool);
        }
    }



    void EndWork()
    {
        isWorking = false;
    }

    protected virtual void SetTarget()
    {
        GameObject result = null;
        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i] != null)
            {
                if (result == null)
                {
                    result = targets[i];
                    continue;
                }
                if (Vector2.Distance(transform.position, result.transform.position) > Vector2.Distance(transform.position, targets[i].transform.position))
                    result = targets[i];
            }
            else
            {
                targets.Remove(targets[i]);
            }
        }
        if (result != null)
        {
            StartCoroutine(Work());
            _target = result;
        }
    }
   

    public virtual void AddTarget(Collider2D collision)
    {
        if (collision.GetComponent<MonsterController>() != null)
        {
            targets.Add(collision.gameObject);
        }
    }

    public virtual void RemoveTarget(Collider2D collision)
    {
        if (collision.GetComponent<MonsterController>() != null)
        {
            targets.Remove(collision.gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        if (stat == null)
            return;
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, stat.range);
    }
}
