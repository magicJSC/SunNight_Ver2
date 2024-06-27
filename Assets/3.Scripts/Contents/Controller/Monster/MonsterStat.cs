using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterStat : Stat
{
    public float Defence { get { return _defence; } set { _defence = value; } }
    [SerializeField]
    float _defence;

    public float Damage { get { return _damage; } set { _damage = value; } }
    [SerializeField]
    float _damage;

    public float attackCool;
    public float range;
    public float Speed { get { return _speed; } set { _speed = value; } }
    [SerializeField]
    float _speed;
}
