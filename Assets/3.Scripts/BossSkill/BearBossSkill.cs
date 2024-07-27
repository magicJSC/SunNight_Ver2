using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BearBossSkill : MonoBehaviour
{
    public float nowHp;
    public float maxHp = 200;
    public Color newColor = Color.red;

    private float moveSpeed = 2f;
    private float atkDmg = 20f;
    private float aktDelay = 1f;
    private bool angry;

    public void Start()
    {
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
            renderer.material.color = newColor;
            nowHp *= 1.5f;
            atkDmg *= 1.5f;
            angry = true;
        }
    }
}