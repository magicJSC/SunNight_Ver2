using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterTargetChekcer : MonoBehaviour
{
    MonsterController monsterController;
    MonsterStat stat;


    void Start()
    {
        monsterController = GetComponentInParent<MonsterController>();
        stat = GetComponentInParent<MonsterStat>();
        GetComponent<CircleCollider2D>().radius = stat.lookRange;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<IGetDamage>() != null && collision.GetComponent<IMonster>() == null)
            monsterController.targetList.Add(collision.transform);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<IGetDamage>() != null && collision.GetComponent<IMonster>() == null)
            monsterController.targetList.Remove(collision.transform);

        monsterController.target = monsterController.SetTarget();
    }
}
