using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stat : MonoBehaviour
{
    public Action<float> hpEvent;


    public float Hp { get { return _hp; } set { _hp = Mathf.Clamp(value,0,value); hpEvent?.Invoke(Hp / maxHP); } }
    [SerializeField]
    float _hp;

    [HideInInspector]
    public float maxHP;

    private void Start()
    {
        maxHP = _hp;
    }
}
