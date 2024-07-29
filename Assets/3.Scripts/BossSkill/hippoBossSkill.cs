using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class hippoBossSkill : MonoBehaviour
{
    public float nowHp;
    public float maxHp = 200f;
    public float healCoolTime = 0f;
    Stat Stat;
    

    public void Start()
    {
        nowHp  = maxHp;
    }

    public void Update()
    {
        if (nowHp < 200)
        {
            healCoolTime += Time.deltaTime;
        }
        if (healCoolTime >= 10f)
        {
            HealSkill();
        }
    }

    private void HealSkill()
    {
        nowHp += 20f;
        healCoolTime = 0;
    }

    public void GetDamage(int damage)
    {
        if(damage >= 30)
        {
            nowHp -= damage;
        }
    }
}
