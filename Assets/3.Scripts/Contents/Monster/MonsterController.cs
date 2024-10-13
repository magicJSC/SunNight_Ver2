using NavMeshPlus.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;

public interface IFly
{

}

public interface IWalk
{

}

public class MonsterController : MonoBehaviour, IMonster
{
    public AssetReferenceGameObject navMeshAsset;

    public Action<GameObject> dieEvent;

    [Header("아이템 드랍")]
    [SerializeField]
    float luck;
    [SerializeField]
    ItemSO meat;
    [SerializeField]
    int coin;

    [HideInInspector]
    public Transform target;
    [HideInInspector]
    public List<Transform> targetList = new List<Transform>();

    NavMeshAgent agent;
    NavMeshSurface navMesh;

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
        Init();
    }

    public virtual void Init()
    {
        stat = GetComponent<MonsterStat>();
        sprite = Util.FindChild<SpriteRenderer>(gameObject, "Sprite", true);
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = stat.Speed;

        navMeshAsset.LoadAssetAsync().Completed += (obj) =>
        {
            navMesh = Instantiate(obj.Result).GetComponent<NavMeshSurface>();
            navMesh.size = new Vector3(stat.lookRange * 2,10,stat.lookRange * 2);
            navMesh.transform.position = transform.position;
            navMesh.BuildNavMesh();
            agent.enabled = false;
        };

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
            if(Managers.Game.tower != null)
                return Managers.Game.tower.transform;
            else
                return null;
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

        if(target == null || Managers.Game.isCantPlay)
        {
            State = Define.State.Idle;
            target = SetTarget();
            return;
        }

        if((transform.position - navMesh.transform.position).magnitude >= stat.lookRange / 1.5f)
        {
            navMesh.transform.position = (Vector2)transform.position;
            navMesh.BuildNavMesh();
        }
        CheckObstacle();

        if (agent != null && agent.isActiveAndEnabled)
        {

            agent.SetDestination(target.transform.position);
        }
        if ((target.transform.position - transform.position).magnitude < stat.attackRange)
            State = Define.State.Idle;

        if (curAtkCool < stat.attackCool)
            curAtkCool += Time.deltaTime;

        if(agent.velocity.x != 0)
            sprite.flipX = agent.velocity.x < 0; 
    }

    protected virtual void OnAttack()
    {

    }

    void CheckObstacle()
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, (target.position - transform.position).normalized, stat.attackRange);
        Debug.DrawRay(transform.position, (target.position - transform.position).normalized * stat.attackRange, Color.red);
      
        foreach(RaycastHit2D hit in hits)
        {
            if(hit.transform.GetComponent<IGetDamage>() != null)
            {
                if (hit.transform.GetComponent<IMonster>() != null)
                    continue;
                target = hit.transform;
                break;
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
           Managers.Game.SpawnItem(meat,1,transform.position);
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
}
