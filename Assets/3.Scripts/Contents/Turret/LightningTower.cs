using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class LightningTower : TurretController, IAttack
{
    public AssetReferenceGameObject lightningAsset;

    GameObject lightningPrefab;

    public int chainCount;

    protected override void Init()
    {
        base.Init();
        lightningAsset.LoadAssetAsync().Completed += (obj) =>
        {
            lightningPrefab = obj.Result;
        };
    }

    public void Attack()
    {
        StartCoroutine(SpawnLightning());
    }

    IEnumerator SpawnLightning()
    {
        if (_target == null)
            yield break;
        Lightning lightning = Instantiate(lightningPrefab,_target.transform).GetComponent<Lightning>();
        lightning.damage = stat.Damage; 
        for (int i = 0;i< chainCount-1; i++)
        {
            yield return new WaitForSeconds(0.1f);
            Transform nextTarget = lightning.GetNextTarget();
            if(nextTarget == null)
                yield break;
            lightning = Instantiate(lightningPrefab, _target.transform).GetComponent<Lightning>();
            lightning.damage = stat.Damage;
        }
    }
}
