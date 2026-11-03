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
        if (FieldSession.Active) StartCoroutine(TrainingDefense());
        else StartSpawn();
    }

    IEnumerator TrainingDefense()
    {
        isFinal=false;
        var content=Resources.Load<ExplorationContent>("Exploration/Content");
        for(int i=0;i<2;i++)
        {
            SpawnMonster(content.dog);
            while(monList.Count>0)
            {
                if(PlayerController.isDie || Managers.Game.tower.GetComponent<Stat>().Hp<=0) yield break;
                yield return null;
            }
            if(i==0) yield return new WaitForSeconds(3);
        }
        isFinal=true;
        Debug.Log("FIELD_BALANCE: tutorial training 2 dogs cleared");
        CompleteIfCleared();
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
        CompleteIfCleared();
    }


    void SpawnMonster(GameObject monsterPrefab)
    {
        int index = Random.Range(0, spawnPos.Count);

        Vector3 position=FieldSession.Active ? Managers.Game.tower.transform.position + Vector3.up*12 : spawnPos[index].position;
        MonsterController monster = Instantiate(monsterPrefab, position, Quaternion.identity).GetComponent<MonsterController>();
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
        if (monList.Count == 0 && isFinal)
        {
            isFinal = false;
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
