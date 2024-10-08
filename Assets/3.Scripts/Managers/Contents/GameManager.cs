using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
  
    public TowerController tower;
    public PlayerController player;
    public BuildController build;
    public MouseController mouse;
    public MapManager grid;
   
    public LightController lightController;

    public bool isKeepingTower;

    public GameObject weapon;

    public bool isHandleUI;

    public bool completeTutorial = false;

    public bool isOpeningStory = false;
    public bool canBuild;
    public bool isCantPlay;

    public bool[] isUnlockTowerPos = new bool[6];


    public void SpawnItem(ItemSO item,int amount,Vector2 pos)
    {
        if (item.itemPrefab != null)
            Instantiate(item.itemPrefab, pos, Quaternion.identity).GetComponent<Item_Pick>().Count = amount;
        else
            Managers.Inven.AddOneItem(item);
    }
}
