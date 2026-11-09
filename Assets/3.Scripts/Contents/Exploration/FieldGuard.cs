using UnityEngine;

public sealed class FieldGuard : MonoBehaviour
{
    public string guardId;
    public Vector2 home;
    public float patrolRadius;
    int patrolSide=1;
    float patrolPause;
    bool returning;
    public bool Returning => returning;
    public bool UpdateLeash(MonsterController monster)
    {
        monster.targetList.RemoveAll(t => t == null || Vector2.Distance(home,t.position)>12);
        if (monster.target != null && Vector2.Distance(home,monster.target.position)>12 || Vector2.Distance(home,transform.position)>12)
        {
            if (!returning) { var stat=GetComponent<MonsterStat>(); stat.Hp=stat.maxHP; }
            returning=true;
        }
        if (!returning)
        {
            if(patrolRadius<=0 || monster.target!=null || monster.State==Define.State.Attack) return false;
            monster.target=monster.SetTarget();
            if(monster.target!=null) return false;
            var rigid=GetComponent<Rigidbody2D>();
            if(Managers.Game.isCantPlay || Time.time<patrolPause)
            { if(monster.State!=Define.State.Idle) monster.State=Define.State.Idle; rigid.linearVelocity=Vector2.zero; return true; }
            Vector2 destination=home+Vector2.right*patrolRadius*patrolSide;
            if(Vector2.Distance(transform.position,destination)<.3f)
            { patrolSide=-patrolSide; patrolPause=Time.time+1.5f; rigid.linearVelocity=Vector2.zero; }
            else
            {
                if(monster.State!=Define.State.Move) monster.State=Define.State.Move;
                rigid.linearVelocity=(destination-(Vector2)transform.position).normalized*GetComponent<MonsterStat>().Speed*.5f;
            }
            return true;
        }
        monster.target=null;
        monster.State=Define.State.Idle;
        var body=GetComponent<Rigidbody2D>();
        body.linearVelocity=(home-(Vector2)transform.position).normalized * GetComponent<MonsterStat>().Speed;
        if (Vector2.Distance(home,transform.position)<.3f) { returning=false; body.linearVelocity=Vector2.zero; }
        return true;
    }
    void Start()
    {
        GetComponent<MonsterController>().dieEvent += OnDeath;
    }
    void OnDeath(GameObject ignored)
    {
        if (!FieldSession.State.killedGuards.Contains(guardId)) FieldSession.State.killedGuards.Add(guardId);
    }
}
