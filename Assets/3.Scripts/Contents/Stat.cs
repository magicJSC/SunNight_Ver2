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
        GameObject go = Util.FindChild(gameObject, "UI_HpBar");
        if(go == null)
        {
            go = Instantiate(Resources.Load<GameObject>("UI/UI_HpBar"),transform);
        }
        hpBar = go.GetComponent<UI_HpBar>();
        maxHP = Hp;
    }
}
