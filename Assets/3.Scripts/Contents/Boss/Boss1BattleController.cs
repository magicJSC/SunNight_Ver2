using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss1BattleController : MonoBehaviour
{
    public WaveSO[] waveSO;
    public Transform[] spawnPosList;

    private void OnEnable()
    {
        StartBossBattle();
    }

    private void OnDisable()
    {
        Managers.Game.canMoveTower = true;
    }

    void StartBossBattle()
    {
        Managers.Game.canMoveTower = false;
        StartSpawn();
    }

    void StartSpawn()
    {
        StartCoroutine(SpawnMonster());
    }

    IEnumerator SpawnMonster()
    {
        int index = Random.Range(0, waveSO.Length);
        for (int i = 0; i < waveSO[index].groups.Count; i++)
        {
            for (int j = 0; j < waveSO[index].groups[i].count; j++)
            {
                int posIndex = Random.Range(0, spawnPosList.Length);
                Instantiate(waveSO[index].groups[i].monster, spawnPosList[posIndex].position,Quaternion.identity);
                yield return new WaitForSeconds(waveSO[index].groups[i].spawnDelay);
            }
            yield return new WaitForSeconds(waveSO[index].groups[i].groupDelay);
        }
        yield return new WaitForSeconds(5);
    }
}
