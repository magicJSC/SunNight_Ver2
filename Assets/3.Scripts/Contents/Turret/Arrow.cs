using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    [HideInInspector]
    public List<GameObject> arrowList;

    int damage;
    Rigidbody2D rigid;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        StartCoroutine(OnDisappear());
    }

    private void Disappear()
    {
        rigid.velocity = Vector3.zero;
        gameObject.SetActive(false);
        arrowList.Add(gameObject);
    }

    public void Shoot(int damage, Vector2 direct,float attackSpeed)
    {
        gameObject.SetActive(true);
        arrowList.Remove(gameObject);
        this.damage = damage;
        rigid.velocity = direct * attackSpeed;
        float rot = Mathf.Atan2(-direct.y, -direct.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, rot+90);
    }

    IEnumerator OnDisappear()
    {
        yield return new WaitForSeconds(3);
        Disappear();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<IMonster>(out var monster))
        {
            monster.GetDamage(damage);
            Disappear();
        }
    }
}
