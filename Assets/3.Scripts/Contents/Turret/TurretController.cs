using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Pipeline.Utilities;
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

    protected override void Init()
    {
        anim = GetComponent<Animator>();
        stat = GetComponent<BuildStat>();
        if (stat == null)
            Debug.Log($"{name}포탑에 TurretStat이 존재하지 않습니다");

        Vector2 tower = Managers.Game.tower.transform.position;
        if (MapManager.cantBuild.HasTile(new Vector3Int((int)(transform.position.x - tower.x), (int)(transform.position.y - tower.y), 0)))
        {
            Item_Buliding building = GetComponentInParent<Item_Buliding>();
            Managers.Inven.AddItems(building.itemSo, 1);
            building.DeleteBuilding();
        }
    }

    private void OnEnable()
    {
        StartCoroutine(UpdateCor());
    }

    IEnumerator UpdateCor()
    {
        while (true)
        {
            if (Managers.Game.isKeepingTower)
            {
                yield return null;
                continue;
            }
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
        yield return new WaitForSeconds(termBeforeWork);
        while (true)
        {
            if (Managers.Game.isKeepingTower)
                yield break;
            if (_target == null)
                yield break;
            isWorking = true;
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
