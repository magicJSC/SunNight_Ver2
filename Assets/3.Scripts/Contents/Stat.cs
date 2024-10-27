using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stat : MonoBehaviour, IGetDamage
{
    public Action<Stat> hpEvent;
    public Action damagedEvent;
    public Action dieEvent;

    public float Hp { get { return _hp; } 
        set
        { 
            if(_hp > value)
                damagedEvent?.Invoke();
            _hp = Mathf.Clamp(value,0,value); 
            hpEvent?.Invoke(this);
        } 
    }
    float _hp;

    public float maxHP;

    bool isDie;

    private void Start()
    {
        _hp = maxHP;
    }

    public void GetDamage(int damage)
    {
        if (isDie)
            return;

        Hp -= damage;
        if(Hp <= 0)
        {
            dieEvent?.Invoke();
            isDie = true;
        }
    }
}
