using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class Flamethrower : TurretController, IAttack,IRotate
{
    public AssetReferenceGameObject flameAsset;

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
        for (int i = 0; i < flameCount; i++)
        {
            flameAsset.InstantiateAsync().Completed += (go) => 
            {
                flameList.Add(go.Result);
                go.Result.GetComponent<Flame>().flameList = flameList;
                go.Result.GetComponent<Flame>().SetDamage(stat.Damage);
                go.Result.SetActive(false);
            };
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
