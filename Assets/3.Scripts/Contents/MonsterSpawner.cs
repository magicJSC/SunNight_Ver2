using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    [SerializeField]
    float range;

    [SerializeField]
    GameObject[] monsters;

    bool canSpawn;

    private void Start()
    {
        TimeController.battleEvent += CheckCanSpawn;
        GetComponent<BoxCollider2D>().size = new Vector2(range,range);
    }

    void CheckCanSpawn()
    {
        if(canSpawn)
            StartSpawn();
    }

    void StartSpawn()
    {
        StartCoroutine(Spawn());
    }

    IEnumerator Spawn()
    {
        while(true)
        {

            if (TimeController.timeType == TimeController.TimeType.Night)
                yield break;

            int monsterIndex = Random.Range(0, monsters.Length);

            Instantiate(monsters[monsterIndex],transform.position,Quaternion.identity);
            yield return new WaitForSeconds(2f);

        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.GetComponent<IPlayer>() != null)
            canSpawn = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<IPlayer>() != null)
            canSpawn = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position,new Vector2(range,range));
    }
}
