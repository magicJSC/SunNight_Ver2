using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    [SerializeField]
    Transform[] spots;
    [SerializeField]
    GameObject[] monsters;
    private void Start()
    {
        Managers.Game.nightEvent -= StartSpawn;
        Managers.Game.nightEvent += StartSpawn;
    }

    void StartSpawn()
    {
        StartCoroutine(Spawn());
    }

    IEnumerator Spawn()
    {
        while (true)
        {
            if (Managers.Game.timeType == Define.TimeType.Morning)
                break;

            int spotIndex = Random.Range(0, spots.Length);
            Vector3 randomPos = new Vector2(Random.Range(-4,4),Random.Range(-4,4));
            int monsterIndex = Random.Range(0, monsters.Length);

            GameObject monster = Instantiate(monsters[monsterIndex]);
            monster.transform.position = randomPos+spots[spotIndex].transform.position;
            yield return new WaitForSeconds(0.8f);
        }
    }
}
