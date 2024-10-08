using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Map : MonoBehaviour
{
    public GameObject[] towerSpot;

    public void OnEnable()
    {
        for(int i = 0;i < towerSpot.Length; i++)
        {
            towerSpot[i].gameObject.SetActive(Managers.Game.isUnlockTowerPos[i]);
        }
    }
}
