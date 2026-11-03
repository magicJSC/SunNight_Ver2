using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Boss1BattleController : MonoBehaviour
{
    public WaveSO[] waveSO;
    public Transform[] spawnPosList;

    bool battleStarted;
    readonly HashSet<GameObject> aliveMonsters = new();

    public void StartBattle()
    {
        if (battleStarted)
            return;

        battleStarted = true;
        StartBossBattle();
    }

    private void OnDisable()
    {
        if (!Managers.HasInstance) return;
        Managers.Game.canMoveTower = true;
    }

    void StartBossBattle()
    {
        Managers.Game.canMoveTower = false;
        StartCoroutine(FieldSession.Active ? RunFieldRounds() : RunRounds());
    }

    IEnumerator RunFieldRounds()
    {
        var content=Resources.Load<ExplorationContent>("Exploration/Content");
        if(content==null || content.dog==null || content.coala==null)
        {Debug.LogError("First-field boss encounter content is missing.");yield break;}
        yield return new WaitForSeconds(5);
        for(int round=0;round<3;round++)
        {
            bool[] ranged=round==0?new[]{false}:round==1?new[]{false,false,false}:new[]{true,false,true,false,false};
            for(int i=0;i<ranged.Length;i++)
            {
                Vector2 offset=i%2==0?Vector2.up*12:Vector2.right*12;
                var go=Instantiate(ranged[i]?content.coala:content.dog,(Vector2)Managers.Game.tower.transform.position+offset,Quaternion.identity);
                var stat=go.GetComponent<MonsterStat>();stat.maxHP=ranged[i]?75:30;
                stat.Damage=ranged[i]?3:5;stat.attackCool=ranged[i]?5:1.5f;stat.Speed=3;
                var monster=go.GetComponent<MonsterController>();monster.targetType=MonsterController.TargetType.Tower;
                monster.getItemList.Clear();aliveMonsters.Add(go);monster.dieEvent+=OnMonsterDied;
                if(i<ranged.Length-1) yield return new WaitForSeconds(2);
            }
            yield return WaitForRoundClear();
            if(round<2) yield return new WaitForSeconds(10);
        }
        var boss=FindAnyObjectByType<Boss1>();
        if(boss!=null) boss.StartBattle();
    }

    IEnumerator RunRounds()
    {
        List<GameObject> monsterPool = GetMonsterPool();
        if (monsterPool.Count == 0)
        {
            Debug.LogError("Boss battle has no monster prefabs configured in WaveSO.");
            yield break;
        }

        yield return RunGroup(monsterPool[Random.Range(0, monsterPool.Count)], 1, 1f);
        yield return WaitForRoundClear();

        for (int i = 0; i < 3; i++)
        {
            GameObject monster = monsterPool[Random.Range(0, monsterPool.Count)];
            yield return RunGroup(monster, 1, 1f);
        }
        yield return WaitForRoundClear();

        List<GameObject> finalRoundMonsters = SelectFinalRoundMonsters(monsterPool);
        yield return RunGroup(finalRoundMonsters[0], 2, 1f);
        yield return RunGroup(finalRoundMonsters[1], 3, 1f);
        yield return WaitForRoundClear();

        Boss1 boss = FindAnyObjectByType<Boss1>();
        if (boss != null)
            boss.StartBattle();
    }

    List<GameObject> GetMonsterPool()
    {
        List<GameObject> monsterPool = new();
        foreach (WaveSO wave in waveSO)
        {
            if (wave == null || wave.groups == null)
                continue;

            foreach (WaveSO.Group group in wave.groups)
            {
                if (group.monster != null && !monsterPool.Contains(group.monster))
                    monsterPool.Add(group.monster);
            }
        }

        return monsterPool;
    }

    List<GameObject> SelectFinalRoundMonsters(List<GameObject> monsterPool)
    {
        List<GameObject> candidates = new(monsterPool);
        for (int i = candidates.Count - 1; i > 0; i--)
        {
            int swapIndex = Random.Range(0, i + 1);
            (candidates[i], candidates[swapIndex]) = (candidates[swapIndex], candidates[i]);
        }

        if (candidates.Count == 1)
            candidates.Add(candidates[0]);

        return candidates;
    }

    IEnumerator RunGroup(GameObject monsterPrefab, int count, float spawnDelay)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnMonster(monsterPrefab);
            if (i < count - 1)
                yield return new WaitForSeconds(spawnDelay);
        }
    }

    IEnumerator WaitForRoundClear()
    {
        while (aliveMonsters.Count > 0)
            yield return null;
    }

    void SpawnMonster(GameObject monsterPrefab)
    {
        if (spawnPosList == null || spawnPosList.Length == 0)
        {
            Debug.LogError("Boss battle has no monster spawn positions configured.");
            return;
        }

        Transform spawnPosition = spawnPosList[Random.Range(0, spawnPosList.Length)];
        if (spawnPosition == null)
            return;

        GameObject monsterObject = Instantiate(monsterPrefab, spawnPosition.position, Quaternion.identity);
        MonsterController monster = monsterObject.GetComponent<MonsterController>();
        if (monster == null)
        {
            Debug.LogError($"Boss wave prefab {monsterPrefab.name} has no MonsterController.");
            Destroy(monsterObject);
            return;
        }

        monster.targetType = MonsterController.TargetType.Tower;
        aliveMonsters.Add(monsterObject);
        monster.dieEvent += OnMonsterDied;
    }

    void OnMonsterDied(GameObject monster)
    {
        aliveMonsters.Remove(monster);
    }
}
