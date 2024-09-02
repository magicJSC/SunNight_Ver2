using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class hedgehogBossSkill : MonoBehaviour
{
    public float nowHp;
    public float maxHp = 200;
    public float defenseTime;
    public float defenseDelay;
    public float atkdelay = 0;
    public GameObject bulletPrefab;
    public bool defensing;
    private float lastShootTime;
    private float ShootTime = float.MaxValue;

    public Transform firingPos;
    public Transform Target;

    private void Start()
    {
        defenseDelay = 10;
        nowHp = maxHp;
        defensing = false;
    }

    private void Update()
    {
        Vector3 vector = Target.transform.position - transform.position;
        Vector2 newPos = Target.transform.position - transform.position;
        float rotZ = Mathf.Atan2(newPos.y, newPos.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, rotZ);
        defenseDelay += Time.deltaTime;
        atkdelay += Time.deltaTime;
        if(atkdelay >= 1.5f)
        {
            Attack();
        }

        if (nowHp <= 100  && defenseDelay >= 20 && defensing == false)
        {
            Defense();
        }

        if (defensing == true)
        {
            defenseTime += Time.deltaTime;
            defenseDelay = 0;
        }

        if (defenseTime >= 5)
        {
            defensing = false;
            defenseTime = 0;
        }
    }

    private void Defense()
    {
        //공격 아예무시 추가 필요, 공격 반사피해 필요
        defensing = true;
    }

    private void Attack()
    {
        Transform _target = GameObject.FindGameObjectWithTag("Player").transform;
        CloneBullet();
        atkdelay = 0;
    }

    private void CloneBullet()
    {
        if (ShootTime - lastShootTime > atkdelay)
        {
            Instantiate(bulletPrefab, firingPos.position, transform.rotation);
            lastShootTime = Time.time;
        }
        ShootTime = Time.time;
    }

}
