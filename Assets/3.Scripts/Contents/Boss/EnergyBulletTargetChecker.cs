using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyBulletTargetChecker : MonoBehaviour
{
    EnergyBullet energyBullet;

    private void Start()
    {
        energyBullet = GetComponentInParent<EnergyBullet>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<IGetDamage>(out var damage))
        {
            energyBullet.getMonsterDamageList.Add(damage);
        }
        if (collision.GetComponent<Boss1>() != null)
        {
            energyBullet.Explosion();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<IGetDamage>(out var damage))
        {
            energyBullet.getMonsterDamageList.Remove(damage);
        }
    }
}
