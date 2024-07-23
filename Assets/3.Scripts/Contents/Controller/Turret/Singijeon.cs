using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singijeon : TurretController, IAttack, IRotate
{
    [SerializeField]
    int arrowCount;
    [SerializeField]
    float attackSpeed;

    GameObject arrow;

    Vector2 targetDirection;
    [HideInInspector]
    public List<GameObject> arrowList = new List<GameObject>();

    protected override void Init()
    {
        base.Init();
        arrow = Resources.Load<GameObject>("Prefabs/Arrow");
        for(int i = 0;i < arrowCount; i++)
        {
           GameObject go = Instantiate(arrow,transform);
           arrowList.Add(go);
            go.GetComponent<Arrow>().arrowList = arrowList;
            go.SetActive(false);
        }
    }

    public void Attack()
    {
        StartCoroutine(SpawnArrow());
    }

    IEnumerator SpawnArrow()
    {
        while (true)
        {
            if(!isWorking)
                yield break;

            for(int i = 0;i < 3;i++)
            {
                GameObject go = arrowList[0];
                Vector2 dir = new Vector2(0.5f,0) * i;
                go.GetComponent<Arrow>().Shoot(stat.Damage, targetDirection - new Vector2(0.5f,0) + dir, attackSpeed);
                go.transform.position = transform.position;
            }
            yield return new WaitForSeconds(0.4f);
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
