using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [SerializeField]
    ItemSO itemSO;

    GameObject item;

    private void Start()
    {
        StartCoroutine(SpawnItem());
    }

    IEnumerator SpawnItem()
    {
        while (true)
        {
            if (item == null)
            {
                yield return new WaitForSeconds(2);
                Vector3Int pos = new Vector3Int((int)transform.position.x, (int)transform.position.y);
                MapManager.matter.SetTile(pos, itemSO.tile);
                item = MapManager.matter.GetInstantiatedObject(pos);
            }
            yield return null;
        }
    }
}
