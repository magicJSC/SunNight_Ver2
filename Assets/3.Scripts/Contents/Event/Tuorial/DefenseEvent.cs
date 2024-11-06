using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefenseEvent : MonoBehaviour
{
    StartButton button;

    public List<Transform> spawnPos;
    public List<GameObject> monList;

    public WaveSO waveSO;

    public GameObject chain;
    public GameObject close;
    public GameObject open;

    bool isFinal;

    private void Start()
    {
        button = Util.FindChild<StartButton>(gameObject, "Button", true);
        button.pushAction += StartDefense;
        close.SetActive(true);
        open.SetActive(false);
    }

    void StartDefense()
    {
        Managers.Game.canMoveTower = false;
        chain.SetActive(false);
        StartSpawn();
    }

    void StartSpawn()
    {
        StartCoroutine(SpawnMonster());
    }

    IEnumerator SpawnMonster()
    {
        isFinal = false;
        for (int i = 0; i < waveSO.groups.Count; i++)
        {
            for (int j = 0; j < waveSO.groups[i].count; j++)
            {
                SpawnMonster(waveSO.groups[i].monster);
                yield return new WaitForSeconds(waveSO.groups[i].spawnDelay);
            }
            yield return new WaitForSeconds(waveSO.groups[i].groupDelay);
        }
        isFinal = true;
    }


    void SpawnMonster(GameObject monsterPrefab)
    {
        int index = Random.Range(0, spawnPos.Count);

        MonsterController monster = Instantiate(monsterPrefab, spawnPos[index].position, Quaternion.identity).GetComponent<MonsterController>();
        monster.targetType = MonsterController.TargetType.Tower;
        monList.Add(monster.gameObject);
        monster.dieEvent += RemoveMonster;
    }

    void RemoveMonster(GameObject monster)
    {
        monList.Remove(monster);
        if (monList.Count == 0 && isFinal)
        {
            ClearTutorial();
        }
    }

    void ClearTutorial()
    {
        Managers.Game.canMoveTower = true;
        close.SetActive(false);
        open.SetActive(true);
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.GetComponent<TowerController>() != null)
        {
            button.isReady = true;
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<TowerController>() != null)
        {
            button.isReady = false;
        }
    }
}
