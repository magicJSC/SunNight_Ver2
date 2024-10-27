using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flame : MonoBehaviour
{
    [HideInInspector]
    public List<GameObject> flameList;

    int damage;
    Rigidbody2D rigid;

    private void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    private void Disappear()
    {
        rigid.velocity = Vector3.zero;
        gameObject.SetActive(false);
        flameList.Add(gameObject);
    }

    public void SetDamage(int damage)
    {
        this.damage = damage;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.TryGetComponent<MonsterController>(out var monster))
        {
            monster.GetDamage(damage);
        }
    }
}
