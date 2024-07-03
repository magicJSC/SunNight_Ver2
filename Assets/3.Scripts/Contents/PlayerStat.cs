using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStat : MonoBehaviour
{
    public Action<float> hpBarEvent;
    public Action<float> energyBarEvent;
    public Action<float> hungerBarEvent;

    public float Hp { get { return _hp; } set { _hp = Mathf.Clamp(value, 0, value); hpBarEvent.Invoke(Hp / maxHP); } }
    [SerializeField]
    float _hp;

    [HideInInspector]
    public float maxHP;

    public float Energy { get { return _energy; } set { _energy = Mathf.Clamp(value, 0, value); energyBarEvent.Invoke(_energy / maxEnergy); } }
    [SerializeField]
    float _energy;

    [HideInInspector]
    public float maxEnergy;

    public float Hunger { get { return _hunger; } set { _hunger = Mathf.Clamp(value, 0, value); hungerBarEvent.Invoke(_hunger / maxHunger); } }
    [SerializeField]
    float _hunger;

    [HideInInspector]
    public float maxHunger;

    private void Start()
    {
        maxHP = Hp;
        maxEnergy = Energy;
        maxHunger = Hunger;
    }
}
