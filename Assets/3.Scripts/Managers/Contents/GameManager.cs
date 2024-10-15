using System;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
  
    public TowerController tower;
    public PlayerController player;
    public BuildController build;
    public MouseController mouse;
    public MapManager grid;
    public TimeController timeController;
   
    public LightController lightController;

    public bool isKeepingTower;

    public GameObject weapon;

    public bool isHandleUI;

    public bool completeTutorial = false;

    public bool isOpeningStory = false;
    public bool canBuild;
    public bool isCantPlay;

    public ChangeSceneEffecter changeSceneEffecter;

    public bool[] isUnlockTowerPos = new bool[6];


    public void SpawnItem(ItemSO item,int amount,Vector3 pos)
    {
         int leftCount = Managers.Inven.AddItems(item,amount);
        if(leftCount > 0)
            Instantiate(item, pos, Quaternion.identity).GetComponent<Item_Pick>().SetInfo(item,amount);
    }
}
