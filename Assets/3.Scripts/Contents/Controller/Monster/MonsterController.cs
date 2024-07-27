using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFly
{

}

public interface IWalk
{

}

public class MonsterController : MonoBehaviour, IMonster
{
    [Header("아이템 드랍")]
    public float luck;
    public ItemSO meat;


    Rigidbody2D rigid;

    protected Transform target;
    List<Transform> targetList = new List<Transform>();

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
                    break;
                case Define.State.Move:
                    anim.Play("Move");
                    break;
                case Define.State.Attack:
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
        rigid = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        curAtkCool = stat.attackCool;
        GetComponent<CircleCollider2D>().radius = stat.lookRange;
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


        if (Vector2.Distance(target.transform.position, transform.position) < stat.attackRange)
        {
            rigid.velocity = Vector2.zero;
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
        CheckObstacle();

        if(target == null)
        {
            State = Define.State.Idle;
            return;
        }

        if (Vector2.Distance(target.transform.position, transform.position) > stat.attackRange)
            rigid.velocity = (target.transform.position - transform.position).normalized * stat.Speed;
        else
            State = Define.State.Idle;

        if (curAtkCool < stat.attackCool)
            curAtkCool += Time.deltaTime;

        sprite.flipX = rigid.velocity.x < 0;
    }

    protected virtual void OnAttack()
    {
        rigid.velocity = Vector2.zero;
    }

    void CheckObstacle()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, (target.position - transform.position).normalized, stat.attackRange);
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
        float index = Random.Range(0, 101);
        if (index <= luck)
        {
            Vector3Int pos = new Vector3Int((int)transform.position.x, (int)transform.position.y);
            MapManager.matter.SetTile(pos, meat.tile);
        }
        Destroy(gameObject);
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Stat>() != null)
            targetList.Add(target);
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<Stat>() != null)
            targetList.Remove(target);

        target = SetTarget();
    }
}
