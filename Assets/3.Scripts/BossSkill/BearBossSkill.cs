using System.Collections;
using System.Collections.Generic;
using UnityEditor.AddressableAssets.Build.Layout;
using UnityEngine;

public class BearBossSkill : MonoBehaviour
{
    MonsterStat MonsterStat;
    public float nowHp;
    public float maxHp = 200;
    public Color newColor = Color.red;

    private float atkDmg = 20f;
    private bool angry;

    public void Start()
    {
        MonsterStat = GetComponent<MonsterStat>();
        angry = false;
        nowHp = maxHp;
        Renderer renderer = GetComponent<Renderer>();
    }

    public void Update()
    {
        if (nowHp <= maxHp * 0.4)
        {
            StatUp();
        }
    }

    private void StatUp()
    {
        if (angry == false)
        {
            Renderer renderer = GetComponent<Renderer>();
            //newColor: 곰의 hp가 40%이하로 내려갈 경우 바뀌는 색
            renderer.material.color = newColor;
            nowHp *= 1.5f;
            atkDmg *= 1.5f;
            angry = true;
        }
    }
}