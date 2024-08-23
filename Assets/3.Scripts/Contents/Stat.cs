using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stat : MonoBehaviour
{
    public Action<Stat> hpEvent;


    public float Hp { get { return _hp; } set { _hp = Mathf.Clamp(value,0,value); hpEvent?.Invoke(this); } }
    float _hp;

    [SerializeField]
    public float maxHP;

    private void Start()
    {
        _hp = maxHP;
    }
}
