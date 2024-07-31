using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public interface IFly
{

}

public interface IWalk
{

}

public class MonsterController : MonoBehaviour, IMonster
{
    public Action<GameObject> dieEvent;

    [Header("아이템 드랍")]
    [SerializeField]
    float luck;
    [SerializeField]
    ItemSO meat;
    [SerializeField]
    int money;

    [HideInInspector]
    public Transform target;
    [HideInInspector]
    public List<Transform> targetList = new List<Transform>();

    NavMeshAgent agent;

    protected Animator anim;

    public Define.State State
    {
        get { return _state; }
        set
        {
            _state = value;
            if (anim == null)
                anim = GetComponent<Animator>();
            switch (_state)
            {
                case Define.State.Idle:
                    anim.Play("Idle");
                    agent.enabled = false;
                    break;
                case Define.State.Move:
                    anim.Play("Move");
                    agent.enabled = true;
                    break;
                case Define.State.Attack:
                    agent.enabled = false;
                    anim.Play("Attack");
                    break;
                case Define.State.Die:
                    anim.Play("Die");
                    break;
            }
        }
    }
    Define.State _state = Define.State.Idle;

    public enum TargetType
    {
        Player,
        Tower
    }
    [HideInInspector]
    public TargetType targetType;

    protected MonsterStat stat;
    protected float curAtkCool;
    protected SpriteRenderer sprite;

    void Start()
    {
        stat = GetComponent<MonsterStat>();
        sprite = GetComponent<SpriteRenderer>();
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = stat.Speed;

        curAtkCool = stat.attackCool;
        StartCoroutine(UpdateCor());
    }


    IEnumerator UpdateCor()
    {
        while (true)
        {
            switch (State)
            {
                case Define.State.Idle:
                    OnIdle();
                    break;
                case Define.State.Move:
                    OnMove();
                    break;
                case Define.State.Attack:
                    OnAttack();
                    break;
            }
            yield return null;
        }
    }

    //저녁과 아침이 목표 우선순위를 다르게 하기
    public Transform SetTarget()
    {
        Transform result = null;
        if (targetType == TargetType.Player)
        {
            for (int i = 0; i < targetList.Count; i++)
            {
                if (targetList[i] == null)
                {
                    targetList.Remove(targetList[i]);
                }

                if (targetList[i].GetComponent<IPlayer>() != null)
                {
                    result = targetList[i];
                    return result;
                }
                else
                    result = targetList[i];
            }

            return result;
        }
        else if (targetType == TargetType.Tower)
            return Managers.Game.tower.transform;

        return null;
    }

    protected virtual void OnIdle()
    {

        if (target == null)
        {
            target = SetTarget();
            return;
        }

        if ((target.transform.position - transform.position).magnitude < stat.attackRange)
        {
            if (curAtkCool >= stat.attackCool)
            {
                curAtkCool = 0;
                State = Define.State.Attack;
                return;
            }
            else
                curAtkCool += Time.deltaTime;
        }
        else
            State = Define.State.Move;
    }
    protected virtual void OnMove()
    {

        if(target == null)
        {
            State = Define.State.Idle;
            return;
        }
        CheckObstacle();

        if(agent.enabled)
            agent.SetDestination(target.transform.position);
      
        if ((target.transform.position - transform.position).magnitude < stat.attackRange)
            State = Define.State.Idle;

        if (curAtkCool < stat.attackCool)
            curAtkCool += Time.deltaTime;

        sprite.flipX = (target.transform.position - transform.position).normalized.x < 0; 
    }

    protected virtual void OnAttack()
    {

    }

    void CheckObstacle()
    {
        if (target == null)
            return;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, (target.position - transform.position).normalized, stat.attackRange,0);
        Debug.DrawRay(transform.position, (target.position - transform.position).normalized * stat.attackRange, Color.red);
        if (hit)
        {
            if (hit.transform.TryGetComponent<IGetDamage>(out var getDamage))
            {
                if (hit.transform.GetComponent<IMonster>() != null)
                    return;
                target = hit.transform;
            }
        }
    }

    void EndAtk()
    {
        State = Define.State.Idle;
    }

    public void GetDamage(int damage)
    {
        stat.Hp -= damage;
        if (stat.Hp <= 0)
            Die();
    }

    public void Die()
    {
        float index = UnityEngine.Random.Range(0, 101);
        if (index <= luck)
        {
            Vector3Int pos = new Vector3Int((int)transform.position.x, (int)transform.position.y);
            MapManager.matter.SetTile(pos, meat.tile);
        }
        dieEvent?.Invoke(gameObject);
        Destroy(gameObject);
    }
}
