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
        Managers.Game.canMoveTower = true;
    }

    void StartBossBattle()
    {
        Managers.Game.canMoveTower = false;
        StartCoroutine(RunRounds());
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
