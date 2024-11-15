using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class EventMonsterSpawner : MonoBehaviour
{
    bool isfinal;

    public WaveSO waveSO;

    float camWidth;
    float camHeight;

    [SerializeField]
    float distance;

    List<GameObject> monList = new List<GameObject>();

    public PlayableDirector cutScene;

    private void Start()
    {
        camHeight = Camera.main.orthographicSize * 2;
        camWidth = camHeight * Camera.main.aspect;
        StartSpawn();
    }

    void StartSpawn()
    {
        StartCoroutine(SpawnMonster());
    }

    IEnumerator SpawnMonster()
    {
        isfinal = false;
        for (int i = 0; i < waveSO.groups.Count; i++)
        {
            for (int j = 0; j < waveSO.groups[i].count; j++)
            {
                NightSpawn(waveSO.groups[i].monster);
                yield return new WaitForSeconds(waveSO.groups[i].spawnDelay);
            }
            yield return new WaitForSeconds(waveSO.groups[i].groupDelay);
        }
        isfinal = true;
    }

    void NightSpawn(GameObject monsterPrefab)
    {
        Vector3 spawnPos = Managers.Game.tower.transform.position;
        float xAmount = Random.Range(-1, 1);
        float yAmount = Random.Range(-1, 1);

        spawnPos += new Vector3(xAmount, yAmount) * distance;

        MonsterController monster = Instantiate(monsterPrefab, spawnPos, Quaternion.identity).GetComponent<MonsterController>();
        monster.targetType = MonsterController.TargetType.Tower;
        monList.Add(monster.gameObject);
        monster.dieEvent += RemoveMonster;
    }

    void RemoveMonster(GameObject monster)
    {
        monList.Remove(monster);
        if (monList.Count == 0 && isfinal)
        {
            Managers.Game.canMoveTower = true;
            cutScene.Play();
        }
    }

}
