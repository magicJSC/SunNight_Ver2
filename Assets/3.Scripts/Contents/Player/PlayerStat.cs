using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStat : MonoBehaviour
{
    public Action damageEvent;
    public Action<float> hpBarEvent;
    public Action<float> energyBarEvent;
    public Action<float> hungerBarEvent;

    CameraController cam;

    public int Hp { get { return hp; } 
        set 
        {
            if (hp > value)
                OnDamageEvent();
            else if(hp < value)
                OnHillEvent();
            hp = Mathf.Clamp(value, 0, value);
            hpBarEvent.Invoke(((float)Hp / (float)maxHP));
        }
    }
    int hp;

    public int maxHP;

    public float Energy { get { return energy; } set { energy = Mathf.Clamp(value, 0, value); energyBarEvent.Invoke(energy / maxEnergy); } }
    float energy;

   
    public float maxEnergy;

    public float Hunger { get { return hunger; } set { hunger = Mathf.Clamp(value, 0, value); hungerBarEvent.Invoke(hunger / maxHunger); } }
    float hunger;

    
    public float maxHunger;

    private void Start()
    {
        cam = Camera.main.GetComponent<CameraController>();
    }

    void OnDamageEvent()
    {
        cam.Shake(0.5f, 0.5f);
        damageEvent?.Invoke();
    }

    void OnHillEvent()
    {
        
    }
}
