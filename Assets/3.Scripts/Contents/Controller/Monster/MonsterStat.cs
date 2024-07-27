using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterStat : Stat
{
    public int Defence { get { return _defence; } set { _defence = value; } }
    [SerializeField]
    int _defence;

    public int Damage { get { return _damage; } set { _damage = value; } }
    [SerializeField]
    int _damage;

    public float attackCool;
    public float attackRange;
    public float lookRange;
    public float Speed { get { return _speed; } set { _speed = value; } }
    [SerializeField]
    float _speed;
}
