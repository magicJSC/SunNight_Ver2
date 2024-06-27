using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildStat : Stat
{
    public float Damage { get { return _damage; } set { _damage = value; } }
    [SerializeField]
    float _damage;

    public float attackCool;
    public float range;
}
