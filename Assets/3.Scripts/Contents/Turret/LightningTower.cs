using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class LightningTower : TurretController, IAttack
{
    public AssetReferenceGameObject lightningAsset;

    public int chainCount;

    public void Attack()
    {
        StartCoroutine(SpawnLightning());
    }

    IEnumerator SpawnLightning()
    {
        if (_target == null)
            yield break;
        Lightning lightning = lightningAsset.InstantiateAsync(_target.transform).Result.GetComponent<Lightning>();
        lightning.damage = stat.Damage;
        lightning.gameObject.name = Util.GetOriginalName(lightning.name);
        for (int i = 0;i< chainCount-1; i++)
        {
            yield return new WaitForSeconds(0.1f);
            Transform nextTarget = lightning.GetNextTarget();
            if(nextTarget == null)
                yield break;
            lightning = lightningAsset.InstantiateAsync(_target.transform).Result.GetComponent<Lightning>();
            lightning.gameObject.name = Util.GetOriginalName(lightning.name);
            lightning.damage = stat.Damage;
        }
    }
}
