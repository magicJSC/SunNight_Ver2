using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [SerializeField]
    GameObject item;

    public int itemCount;

    public float range;


    private void Start()
    {
        TimeController.morningEvent += SpawnItem;
        SpawnItem();
    }

    void SpawnItem()
    {
        for(int i = 0; i < itemCount; i++)
        {
            Vector3Int pos = new Vector3Int((int)(transform.position.x + Random.Range(-range, range)), (int)(transform.position.y + Random.Range(-range, range)));
            Instantiate(item,pos,Quaternion.identity);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector2(range, range));
    }

}
