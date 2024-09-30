using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NightMonsterSpawner : MonoBehaviour
{
    [SerializeField]
    float range;

    public WaveSO[] waveSO;

    float camWidth;
    float camHeight;

    [SerializeField]
    float distance;

    List<GameObject> monList = new List<GameObject>();

    private void Start()
    {
        TimeController.nightEvent += StartSpawn;
        camHeight = Camera.main.orthographicSize * 2;
        camWidth = camHeight * Camera.main.aspect;
    }

    void StartSpawn()
    {
        StartCoroutine(SpawnMonster());
    }

    IEnumerator SpawnMonster()
    {
        for(int i = 0; i < waveSO[TimeController.day].groups.Count; i++)
        {
            for(int j = 0;j < waveSO[TimeController.day].groups[i].count; j++)
            {
                NightSpawn(waveSO[TimeController.day].groups[i].monster);
                yield return new WaitForSeconds(waveSO[TimeController.day].groups[i].spawnDelay);
            }
            yield return new WaitForSeconds(waveSO[TimeController.day].groups[i].groupDelay);
        }
    }

    void NightSpawn(GameObject monsterPrefab)
    {
        Vector2 spawnPos = Managers.Game.tower.transform.position;
        float xAmount = Random.Range(-range, range);
        float yAmount = Random.Range(-range, range);

        spawnPos += new Vector2(xAmount, yAmount);

        Vector2 camPos = Camera.main.transform.position;

        Vector3 direction = (spawnPos - camPos).normalized;

        spawnPos = Managers.Game.tower.transform.position + direction * distance;

        if (spawnPos.x < camPos.x + camWidth / 2 && spawnPos.x > camPos.x - camWidth / 2 && spawnPos.y < camPos.y + camHeight / 2 && spawnPos.y > camPos.y - camHeight / 2)
        {
            if (spawnPos.x > camPos.x)
                spawnPos += new Vector2(camWidth / 2, 0);
            else
                spawnPos -= new Vector2(camWidth / 2, 0);
        }

        MonsterController monster = Instantiate(monsterPrefab, spawnPos, Quaternion.identity).GetComponent<MonsterController>();
        monster.targetType = MonsterController.TargetType.Tower;
        monster.dieEvent += RemoveMonster;
    }

    void RemoveMonster(GameObject monster)
    {
        monList.Remove(monster);
        if(monList.Count == 0)
        {
            Debug.Log("Wave ³¡");
        }
    }
}
