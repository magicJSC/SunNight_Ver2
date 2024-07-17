using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildStat : Stat
{
    public int Damage { get { return _damage; } set { _damage = value; } }
    [SerializeField]
    int _damage;

    public float attackCool;
    public float range;
}
