using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class PlayerStat : MonoBehaviour
{
    public Action damageEvent;
    public Action<float> hpBarEvent;
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
            hpBarEvent?.Invoke(((float)Hp / (float)maxHP));
        }
    }
    int hp;

    public int maxHP;

    public float Hunger { get { return hunger; } 
        set
        { 
            hunger = Mathf.Clamp(value, 0, value);
            if (hunger <= maxHunger / 7)
            {
                if(hungerBuff == null)
                 GetHungerBuff();
            }
            else
            {
                if (hungerBuff != null)
                    Destroy(hungerBuff);
            }
            hungerBarEvent?.Invoke(hunger / maxHunger);
        }
    }
    float hunger;

    
    public float maxHunger;

    public float Speed;

    public AssetReferenceGameObject hungerBuffAsset;

    GameObject hungerBuffPrefab;
    GameObject hungerBuff;

    bool isGetting;

    private void Start()
    {
        cam = Camera.main.GetComponent<CameraController>();
       

        StartCoroutine(ReduceHunger());
    }

    void GetHungerBuff()
    {
        if (hungerBuffPrefab == null)
        {
            if (isGetting)
                return;
            isGetting = true;
            hungerBuffAsset.LoadAssetAsync().Completed += (obj) =>
            {
                hungerBuffPrefab = obj.Result;
                hungerBuff = Instantiate(hungerBuffPrefab, transform);
            };
        }
        else
            hungerBuff = Instantiate(hungerBuffPrefab, transform);
    }


    IEnumerator ReduceHunger()
    {
        while(true)
        {
            yield return null;
            if (Hunger <= 0)
                continue;

            Hunger -= Time.deltaTime * maxHunger / 300;
        }
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
