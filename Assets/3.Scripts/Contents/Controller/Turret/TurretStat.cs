using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretStat : Stat
{
    public float Dmg { get { return _dmg; } set { _dmg = value; } }
    [SerializeField]
    float _dmg;

    public float _atkCool;
    public float _range;
}
