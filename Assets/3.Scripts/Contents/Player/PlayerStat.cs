using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStat : MonoBehaviour
{
    public Action<float> hpBarEvent;
    public Action<float> energyBarEvent;
    public Action<float> hungerBarEvent;

    public int Hp { get { return hp; } set { hp = Mathf.Clamp(value, 0, value); Debug.Log(hp); hpBarEvent.Invoke(Hp / maxHP); } }
    [SerializeField]
    int hp;

    [HideInInspector]
    public int maxHP;

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
