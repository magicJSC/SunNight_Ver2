using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MonsterWaveController : MonoBehaviour
{
    bool isfinal;
    bool waveRunning;


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
        if (waveRunning) return;
        waveRunning = true;
        if (FieldSession.Active) { StartCoroutine(SpawnFieldNight()); return; }
        StartCoroutine(SpawnMonster());
    }

    IEnumerator SpawnFieldNight()
    {
        var content=Resources.Load<ExplorationContent>("Exploration/Content");
        if(content==null) { Debug.LogError("First night content missing"); yield break; }
        isfinal=false;
        var rounds=new[] {
            new[]{false,false,false,false},
            new[]{false,false,false,false,true},
            new[]{true,false,false,true,false,false}};
        yield return new WaitForSeconds(5);
        for(int round=0;round<rounds.Length;round++)
        {
            for(int i=0;i<rounds[round].Length;i++)
            {
                if(PlayerController.isDie || Managers.Game.tower.GetComponent<Stat>().Hp<=0) yield break;
                NightSpawn(rounds[round][i] ? content.coala : content.dog,
                    round==0 || i%2==0 ? Vector2.up : Vector2.right);
                if(i<rounds[round].Length-1) yield return new WaitForSeconds(2);
            }
            while(monList.Count>0)
            {
                if(PlayerController.isDie || Managers.Game.tower.GetComponent<Stat>().Hp<=0) yield break;
                yield return null;
            }
            if(round<rounds.Length-1) yield return new WaitForSeconds(10);
        }
        isfinal=true;
        Debug.Log("FIELD_BALANCE: field night 12 dogs / 3 koalas cleared");
        CompleteIfCleared();
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
        CompleteIfCleared();
    }

    void NightSpawn(GameObject monsterPrefab, Vector2? approach = null)
    {
        Vector3 spawnPos = Managers.Game.tower.transform.position;
        // Integer Range(-1,1) only yielded -1 or 0, including a spawn on the tower.
        Vector2 direction = approach ?? Random.insideUnitCircle;
        if (direction.sqrMagnitude < .001f) direction = Vector2.up;
        spawnPos += (Vector3)(direction.normalized * (approach.HasValue ? 12 : distance));


        MonsterController monster = Instantiate(monsterPrefab, spawnPos, Quaternion.identity).GetComponent<MonsterController>();
        monster.targetType = MonsterController.TargetType.Tower;
        monList.Add(monster.gameObject);
        monster.dieEvent += RemoveMonster;
    }

    void RemoveMonster(GameObject monster)
    {
        monList.Remove(monster);
        CompleteIfCleared();
    }

    void CompleteIfCleared()
    {
        if(monList.Count == 0 && isfinal && !PlayerController.isDie && Managers.Game.tower.GetComponent<Stat>().Hp>0)
        {
            waveRunning = false;
            Managers.Game.canMoveTower = true;
            if (FieldSession.Active && !FieldSession.State.firstNightCleared) ExplorationRuntime.GrantFirstNightReward();
        }
    }


}
