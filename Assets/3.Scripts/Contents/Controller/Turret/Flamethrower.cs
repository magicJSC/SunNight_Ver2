using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flamethrower : TurretController, IAttack,IRotate
{
    [SerializeField]
    int flameCount;
    [SerializeField]
    float attackSpeed;
    [SerializeField]
    Transform face;


    Vector2 targetDirection;

    [HideInInspector]
    public List<GameObject> flameList = new List<GameObject>();

    protected override void Init()
    {
        base.Init();
        GameObject flame = Resources.Load<GameObject>("Prefabs/Flame");
        for(int i = 0; i < flameCount; i++)
        {
            GameObject go = Instantiate(flame);
            flameList.Add(go);
            go.GetComponent<Flame>().flameList = flameList;
            go.GetComponent<Flame>().SetDamage(stat.Damage);
            go.SetActive(false);
        }
    }

    public void Attack()
    {
        StartCoroutine(SpawnFlame());
    }

    IEnumerator SpawnFlame()
    {
        while (true) 
        {
            if(!isWorking)
                yield break;
            if(_target == null)
                yield break;

            GameObject flame = flameList[0];
            flameList.Remove(flame);
            flame.SetActive(true);
            flame.transform.position = transform.position;
            flame.GetComponent<Rigidbody2D>().velocity = targetDirection * attackSpeed;
            yield return new WaitForSeconds(0.2f);
        }
    }

    protected override void CheckTarget()
    {
        base.CheckTarget();

        if (_target != null)
            Rotate();
    }

    public void Rotate()
    {
        targetDirection = (_target.transform.position - transform.position).normalized;
        float rot = Mathf.Atan2(-targetDirection.y, -targetDirection.x) * Mathf.Rad2Deg;
        //face.rotation = Quaternion.Euler(0, 0, rot + 90);
    }
}
