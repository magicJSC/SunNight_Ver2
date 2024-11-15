using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MonsterWaveController : MonoBehaviour
{
    bool isfinal;


    public WaveSO[] waveSO;

    [SerializeField]
    float distance;

    List<GameObject> monList = new List<GameObject>();

    TimeController timeController;

    private void Start()
    {
        timeController = Managers.Game.timeController;
        timeController.nightEvent += StartSpawn;
    }

    void StartSpawn()
    {
        StartCoroutine(SpawnMonster());
    }

    IEnumerator SpawnMonster()
    {
        isfinal = false;
        int day = Mathf.Clamp(timeController.day - 1, 0, waveSO.Length - 1);
        for(int i = 0; i < waveSO[day].groups.Count; i++)
        {
            for(int j = 0;j < waveSO[day].groups[i].count; j++)
            {
                NightSpawn(waveSO[day].groups[i].monster);
                yield return new WaitForSeconds(waveSO[day].groups[i].spawnDelay);
            }
            yield return new WaitForSeconds(waveSO[day].groups[i].groupDelay);
        }
        isfinal = true;
    }

    void NightSpawn(GameObject monsterPrefab)
    {
        Vector3 spawnPos = Managers.Game.tower.transform.position;
        float xAmount = Random.Range(-1, 1);
        float yAmount = Random.Range(-1,1);

        spawnPos += new Vector3(xAmount, yAmount) * distance;


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
            Managers.Game.canMoveTower = true;
        }
    }


}
