using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretTargetChecker : MonoBehaviour
{
    TurretController turretController;
    BuildStat stat;

    private void Start()
    {
        turretController = GetComponentInParent<TurretController>();
        stat = GetComponentInParent<BuildStat>();
        GetComponent<CircleCollider2D>().radius = stat.range;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        turretController.AddTarget(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        turretController.RemoveTarget(collision);
    }
}
