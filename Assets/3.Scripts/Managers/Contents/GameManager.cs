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

    public bool isMovingTower;

    public GameObject weapon;

    public bool isHandleUI;

    public bool canMoveTower;

    public bool isOpeningStory = false;
    public bool isCantPlay;

    public UI_Build buildUI;

    public ChangeSceneEffecter changeSceneEffecter;



    public void GetItem(ItemSO item,int amount,Vector3 pos)
    {
         int leftCount = Managers.Inven.GetItem(item,amount);
        if(leftCount > 0)
            Instantiate(item, pos, Quaternion.identity).GetComponent<Item_Pick>().SetInfo(item,amount);

        Managers.Inven.CheckHotBarChoice();
    }
}
