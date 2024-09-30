using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapSpawner : MonoBehaviour
{
    [SerializeField] GameObject[] map;

    private void Awake()
    {
        Transform mapParent = Util.FindChild<Transform>(gameObject,"Maps",true);
        for(int i = 0; i < map.Length; i++)
        {
            Instantiate(map[i],mapParent);
        }
    }
}
