using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterStat : Stat
{
   
    public float Def { get { return _def; } set { _def = value; } }
    [SerializeField]
    float _def;

    public float Dmg { get { return _dmg; } set { _dmg = value; } }
    [SerializeField]
    float _dmg;

    public float _atkCool;
    public float _range;
    public float Speed { get { return _speed; } set { _speed = value; } }
    [SerializeField]
    float _speed;

    
}
