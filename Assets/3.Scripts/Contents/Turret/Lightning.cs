using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lightning : MonoBehaviour
{
    List<Transform> targets = new List<Transform>();


    public int damage;

    private void Start()
    {
        if(transform.parent.TryGetComponent<IMonster>(out var monster))
        {
            monster.GetDamage(damage);
        }
    }

    public Transform GetNextTarget()
    {
        Transform result = null;
        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i] != null)
            {
                if (result == null)
                {
                    result = targets[i];
                    continue;
                }
                if (Vector2.Distance(transform.position, result.position) > Vector2.Distance(transform.position, targets[i].position))
                    result = targets[i];
            }
        }
        return result;
    }

    void Disappear()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.GetComponent<IMonster>() != null && collision != transform.parent)
        {
            if (Util.FindChild(collision.gameObject, "Lightning"))
                return;

            targets.Add(collision.transform);
        }
    }
}
