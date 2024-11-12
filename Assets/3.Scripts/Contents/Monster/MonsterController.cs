
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public interface IFly
{

}

public interface IWalk
{

}

public class MonsterController : MonoBehaviour, IGetPlayerDamage, IKnockBack
{

    public Action<GameObject> dieEvent;

    [Serializable]
    public struct GetItem
    {
        public ItemSO item;
        public int count;
        public float probability;
    }

    public List<GetItem> getItemList = new List<GetItem>();
    [SerializeField]
    int coin;

    [HideInInspector]
    public Transform target;
    [HideInInspector]
    public List<Transform> targetList = new List<Transform>();

    bool isKnockBack;

    Rigidbody2D rigid;
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
                    rigid.velocity = Vector2.zero;
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

    public int endTime { get { return 1; } }

    float curEndTime;

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
        Init();
    }

    public virtual void Init()
    {
        stat = GetComponent<MonsterStat>();
        stat.dieEvent += Die;
        sprite = Util.FindChild<SpriteRenderer>(gameObject, "Sprite", true);
      
        rigid = GetComponent<Rigidbody2D>();
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
        int count = targetList.Count;
        targetList = targetList.OrderBy(target => (target.position - transform.position).magnitude).ToList();
        if (targetType == TargetType.Player)
        {
            for (int i = 0; i < count;)
            {
                if (targetList[i] == null)
                {
                    targetList.Remove(targetList[i]);
                    continue;
                }  
                return result = targetList[i];
            }
        }
        else if (targetType == TargetType.Tower)
        {
            if (target == null)
            {
                return Managers.Game.tower.transform;
            }
            else
            {
                for (int i = 0; i < count;)
                {
                    if (targetList[i] == null)
                    {
                        targetList.Remove(targetList[i]);
                        continue;
                    }
                    return result = targetList[i];
                }
            }
        }
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
        if (isKnockBack)
            return;
        if(target == null || Managers.Game.isCantPlay)
        {
            State = Define.State.Idle;
            target = SetTarget();
            return;
        }

        rigid.velocity = (target.transform.position - transform.position).normalized * stat.Speed;
        

      
        if ((target.transform.position - transform.position).magnitude < stat.attackRange)
            State = Define.State.Idle;

        if (curAtkCool < stat.attackCool)
            curAtkCool += Time.deltaTime;

        if (rigid.velocity.x != 0)
            sprite.flipX = rigid.velocity.x < 0;
    }

    public void GetDamage(float damage)
    {
        stat.GetDamage(damage);
    }
    protected virtual void OnAttack()
    {

    }

    void EndAtk()
    {
        State = Define.State.Idle;
    }

    public void Die()
    {
        int randomIndex = Random.Range(1, 101);
        float num = 0;
        for (int i = 0; i < getItemList.Count; i++)
        {
            if (num < randomIndex && randomIndex <= num + getItemList[i].probability)
            {
                Managers.Game.GetItem(getItemList[i].item, getItemList[i].count, transform.position);
                break;
            }
            num += getItemList[i].probability;
        }
        Managers.Inven.Coin += coin;
        dieEvent?.Invoke(gameObject);
        Destroy(gameObject);
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position,8);
    }

    public void StartKnockBack(Transform attacker)
    {
        curEndTime = 0;
        rigid.velocity = (transform.position - attacker.position).normalized * 5;
        StartCoroutine(KnockBack());
    }

    public IEnumerator KnockBack()
    {
        isKnockBack = true;
        while (true)
        {
            yield return null;
            if (rigid.velocity.magnitude > 0.01f && curEndTime < endTime)
            {
                rigid.velocity = Vector2.Lerp(rigid.velocity, Vector2.zero, 0.1f);
                curEndTime += Time.deltaTime;
            }
            else
                break;
        }
        isKnockBack = false;
    }
}
