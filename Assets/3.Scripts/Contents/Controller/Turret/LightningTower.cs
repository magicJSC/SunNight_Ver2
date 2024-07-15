using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningTower : TurretController, IAttack
{
    public int chainCount;

    public void Attack()
    {
        StartCoroutine(SpawnLightning());
    }

    IEnumerator SpawnLightning()
    {
        Lightning lightning = Instantiate(Resources.Load<GameObject>("Prefabs/Lightning"),_target.transform).GetComponent<Lightning>();
        lightning.damage = stat.Damage;
        for (int i = 0;i< chainCount-1; i++)
        {
            yield return new WaitForSeconds(0.1f);
            Transform nextTarget = lightning.GetNextTarget();
            if(nextTarget == null)
                yield break;
            lightning = Instantiate(Resources.Load<GameObject>("Prefabs/Lightning"), nextTarget.transform).GetComponent<Lightning>();
            lightning.damage = stat.Damage;
        }
    }
}
