using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MonsterWaveController : MonoBehaviour
{
    public static Action surviveEvent;

    bool isfinal;

    [SerializeField]
    float range;

    public WaveSO[] waveSO;

    float camWidth;
    float camHeight;

    [SerializeField]
    float distance;

    List<GameObject> monList = new List<GameObject>();

    TimeController timeController;

    private void Start()
    {
        timeController = Managers.Game.timeController;
        timeController.nightEvent += StartSpawn;
        camHeight = Camera.main.orthographicSize * 2;
        camWidth = camHeight * Camera.main.aspect;
    }

    void StartSpawn()
    {
        StartCoroutine(SpawnMonster());
    }

    IEnumerator SpawnMonster()
    {
        isfinal = false;
        for(int i = 0; i < waveSO[timeController.day - 1].groups.Count; i++)
        {
            for(int j = 0;j < waveSO[timeController.day - 1].groups[i].count; j++)
            {
                NightSpawn(waveSO[timeController.day - 1].groups[i].monster);
                yield return new WaitForSeconds(waveSO[timeController.day - 1].groups[i].spawnDelay);
            }
            yield return new WaitForSeconds(waveSO[timeController.day - 1].groups[i].groupDelay);
        }
        isfinal = true;
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
        monList.Add(monster.gameObject);
        monster.dieEvent += RemoveMonster;
    }

    void RemoveMonster(GameObject monster)
    {
        monList.Remove(monster);
        if(monList.Count == 0 && isfinal)
        {
            StartCoroutine(ClearWave());
           surviveEvent?.Invoke();
        }
    }

    IEnumerator ClearWave()
    {
        Managers.Game.changeSceneEffecter.StartChangeScene();
        yield return new WaitForSeconds(3);
        timeController.day++;
        timeController.dayEvent.Invoke(timeController.day);
        timeController.TimeAmount = 330;
        Managers.Game.changeSceneEffecter.EndChangeScene();
    }

}
