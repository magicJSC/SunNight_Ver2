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

    [Header("Normal")]
    [SerializeField]
    int maxMonsterAmount;

    [Header("Night")]
    [SerializeField]
    float distance;

    float camWidth;
    float camHeight;

    private void Start()
    {
        GetComponent<BoxCollider2D>().size = new Vector2(range,range);
        camHeight = Camera.main.orthographicSize*2;
        camWidth = camHeight * Camera.main.aspect;
        StartNormalSpawn();
    }

    void StartNormalSpawn()
    {
        StartCoroutine(NormalSpawnCoroutine());
    }

    void StartBattleSpawn()
    {
        StartCoroutine(NightSpawnCoroutine());
    }

    List<GameObject> monsterList = new List<GameObject>();

    IEnumerator NormalSpawnCoroutine()
    {
        while(true)
        {
            if(monsterList.Count < maxMonsterAmount)
            {
                yield return new WaitForSeconds(5f);
                monsterList.Add(NormalSpawn());
            }
            yield return null;
        }
    }

    IEnumerator NightSpawnCoroutine()
    {
        while (true)
        {

            if (TimeController.timeType == TimeController.TimeType.Night)
                yield break;

            NightSpawn();
            yield return new WaitForSeconds(2f);
        }
    }

    GameObject NormalSpawn()
    {
        int monsterIndex = Random.Range(0, monsters.Length);

        Vector2 spawnPos = transform.position;
        float xAmount = Random.Range(-range / 2,range / 2);
        float yAmount = Random.Range(-range / 2,range / 2);

        spawnPos = spawnPos + new Vector2(xAmount, yAmount);

        Vector2 camPos = Camera.main.transform.position;

        if(spawnPos.x < camPos.x + camWidth /2 && spawnPos.x > camPos.x - camWidth/2 && spawnPos.y < camPos.y + camHeight/2 && spawnPos.y > camPos.y - camHeight/2)
        {
            if (spawnPos.x > camPos.x)
                spawnPos = spawnPos + new Vector2(camWidth / 2, 0);
            else
                spawnPos = spawnPos - new Vector2(camWidth / 2, 0);
        }
        

        GameObject monster = Instantiate(monsters[monsterIndex], spawnPos, Quaternion.identity);
        monster.GetComponent<MonsterController>().dieEvent += RemoveMonster;

        return monster;
    }

    void NightSpawn()
    {
        int monsterIndex = Random.Range(0, monsters.Length);
        Vector2 spawnPos = Managers.Game.tower.transform.position;
        float xAmount = Random.Range(-range, range);
        float yAmount = Random.Range(-range, range);

        spawnPos = spawnPos + new Vector2(xAmount, yAmount);

        Vector2 camPos = Camera.main.transform.position;

        Vector3 direction = (spawnPos - camPos).normalized;

        spawnPos = Managers.Game.tower.transform.position + direction * distance;

        if (spawnPos.x < camPos.x + camWidth / 2 && spawnPos.x > camPos.x - camWidth / 2 && spawnPos.y < camPos.y + camHeight / 2 && spawnPos.y > camPos.y - camHeight / 2)
        {
            if (spawnPos.x > camPos.x)
                spawnPos = spawnPos + new Vector2(camWidth / 2, 0);
            else
                spawnPos = spawnPos - new Vector2(camWidth / 2, 0);
        }

        Instantiate(monsters[monsterIndex], spawnPos, Quaternion.identity);
    }

    void RemoveMonster(GameObject monster)
    {
        monsterList.Remove(monster);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        TimeController.battleEvent = StartBattleSpawn; 
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position,new Vector2(range,range));
    }
}
