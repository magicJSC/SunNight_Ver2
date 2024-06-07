using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterController : MonoBehaviour
{
    Rigidbody2D rigid;
    public LayerMask playerLayer;
    public LayerMask buildLayer;

    protected Transform target;
    protected Animator anim;

    public Define.State State { get { return _state; } 
        set 
        {
            _state = value;
            if(anim == null)
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

    enum OnFight 
    {
        None,
        Battle
    }
    OnFight onfight;

    protected MonsterStat _stat;
    protected float curAtkCool;
    protected SpriteRenderer sprite;

    void Start()
    {
        _stat = GetComponent<MonsterStat>();
        rigid = GetComponent<Rigidbody2D>();
        sprite =GetComponent<SpriteRenderer>();
        curAtkCool = _stat._atkCool;
    }

    
    void Update()
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
    }

    //저녁과 아침이 목표 우선순위를 다르게 하기
    public Transform SetTarget()
    {
        if (Define.KeyType.Exist != Managers.Inven.hotBar_itemInfo[Managers.Inven.hotBar_itemInfo.Length - 1].keyType)
            return Managers.Game.tower.transform;
        else
            return Managers.Game.player.transform;
    }

    protected virtual void OnIdle()
    {
        if (target != null)
            onfight = OnFight.Battle;
        else
            target = SetTarget();

       if(Define.KeyType.Exist == Managers.Inven.hotBar_itemInfo[Managers.Inven.hotBar_itemInfo.Length - 1].keyType)
            target = SetTarget();

        if (Vector2.Distance(target.transform.position, transform.position) < _stat._range)
        {
            rigid.velocity = Vector2.zero;
            if (curAtkCool >= _stat._atkCool)
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

        if (target != null)
            onfight = OnFight.Battle;
        else
            target = Managers.Game.tower.transform;

        if(onfight == OnFight.Battle)
        {
            if (Vector2.Distance(target.transform.position, transform.position) > _stat._range)
                rigid.velocity = (target.transform.position - transform.position).normalized * _stat.Speed;
            else
                State = Define.State.Idle;

            if (curAtkCool < _stat._atkCool)
                curAtkCool += Time.deltaTime;
        }

        sprite.flipX = rigid.velocity.x < 0; 
    }
    
    protected virtual void OnAttack()
    {
        rigid.velocity = Vector2.zero;
    }

    void CheckObstacle()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, (target.position - transform.position).normalized, _stat._range, buildLayer);
        Debug.DrawRay(transform.position, (target.position - transform.position).normalized * _stat._range,Color.red);
        if(hit)
        {
            target = hit.transform;
        }
    }

    void EndAtk()
    {
        State = Define.State.Idle;
    }
}
