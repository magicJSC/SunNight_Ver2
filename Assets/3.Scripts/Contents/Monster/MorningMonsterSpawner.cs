using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class MorningMonsterSpawner : MonoBehaviour
{
    [SerializeField]
    float range;

    [SerializeField]
    GameObject monsterPrefab;

    GameObject monster;

    private void Start()
    {
        TimeController.morningEvent += NormalSpawn;
    }


    void NormalSpawn()
    {
        if (monster != null)
            return;
        Vector2 spawnPos = transform.position;
        monster = Instantiate(monsterPrefab, spawnPos, Quaternion.identity);
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position,new Vector2(range,range));
    }
}
