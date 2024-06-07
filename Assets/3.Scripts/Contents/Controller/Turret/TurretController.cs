using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretController : MonoBehaviour
{
    TurretStat _stat;
    Animator anim;


    List<GameObject> _targets = new List<GameObject>();
    GameObject _target;
    Transform face;

    float atkCurTime = 0;
    bool isWorking = false;
    private void Start()
    {
        anim = GetComponent<Animator>();
        _stat = GetComponent<TurretStat>();
        if (_stat == null)
            Debug.Log($"{name}포탑에 TurretStat이 존재하지 않습니다");
        GetComponent<CircleCollider2D>().radius = _stat._range;
        GetComponent<CircleCollider2D>().isTrigger = true;
        face = Util.FindChild(gameObject, "Face", true).transform;
    }

    private void Update()
    {
        if (isWorking)
            return;
        Rotate();
    }

    void Rotate()
    {
        if (_target == null)
        {
            SetTarget();
            return;
        }
        else if (Vector2.Distance(_target.transform.position, transform.position) > _stat._range)
        {
            SetTarget();
            return;
        }
        Vector3 v = (_target.transform.position - transform.position).normalized;
        face.up = Vector3.Lerp(face.up, v, 0.5f);
        Attack();
    }

    protected virtual void Attack()
    {
        if (atkCurTime > _stat._atkCool)
        {
            isWorking = true;
            atkCurTime = 0;
            anim.Play("Attack");
            _target.GetComponent<MonsterStat>().Hp -= _stat.Dmg;
            if (_target.GetComponent<MonsterStat>().Hp <= 0)
            {
                _targets.Remove(_target);
                Destroy(_target);
            }
        }
        else
            atkCurTime += Time.deltaTime;
    }

    void EndAtk()
    {
        isWorking = false;
    }

    protected virtual void SetTarget()
    {
        GameObject result = null;
        for (int i = 0; i < _targets.Count; i++)
        {
            if (_targets[i] != null)
            {
                if (result == null)
                {
                    result = _targets[i];
                    continue;
                }
                if (Vector2.Distance(transform.position, result.transform.position) > Vector2.Distance(transform.position, _targets[i].transform.position))
                    result = _targets[i];
            }
            else
            {
                _targets.Remove(_targets[i]);
            }
        }
        _target = result;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<MonsterController>() != null)
        {
            _targets.Add(collision.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<MonsterController>() != null)
        {
            _targets.Remove(collision.gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        if (_stat == null)
            return;
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, _stat._range);
    }
}
