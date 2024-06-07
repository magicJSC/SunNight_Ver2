using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stat : MonoBehaviour
{
    UI_HpBar hpBar;
    public float Hp { get { return _hp; } set { _hp = Mathf.Clamp(value,0,value); hpBar.SetHp(Hp / maxHP); } }
    [SerializeField]
    float _hp;

    [HideInInspector]
    public float maxHP;

    private void Start()
    {
        hpBar = Util.FindChild(gameObject,"UI_HpBar").GetComponent<UI_HpBar>();
        maxHP = Hp;
    }
}
