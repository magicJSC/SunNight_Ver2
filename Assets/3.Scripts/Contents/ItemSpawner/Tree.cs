using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : MonoBehaviour
{
    ItemSO branch;
     
    void Start()
    {
        branch = Resources.Load<Item_Pick>("Prefabs/Items/Branch").itemSo;

       
        for(int i = 0;i < 5;i++)
        {
            float x = Mathf.Round(transform.position.x + Random.Range(-2,3));
            float y = Mathf.Round(transform.position.y + Random.Range(-2,-3));
            Managers.Game.SpawnItem(branch,1,new Vector2(x,y));          
        }
    }
}
