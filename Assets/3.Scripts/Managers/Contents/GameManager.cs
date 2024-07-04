using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Define;

public class GameManager : MonoBehaviour
{
    public void Init()
    {
        if(grid == null)
        {
            grid = FindAnyObjectByType<MapManager>();
            if(grid == null)
            {
                grid = Instantiate(Resources.Load<GameObject>("Prefabs/Grid")).GetComponent<MapManager>();
            }
            grid.Init();
        }
        if (mouse == null)
        {
            mouse = FindAnyObjectByType<MouseController>();
            if (mouse == null)
            {
                mouse = Resources.Load<GameObject>("Prefabs/Mouse").GetComponent<MouseController>();
            }
            mouse.Init();
        }
        if (build == null)
        {
            build = FindAnyObjectByType<BuildController>();
            if (build == null)
            {
                build = Instantiate(Resources.Load<GameObject>("Prefabs/Builder")).GetComponent<BuildController>();
            }
            build.Init();
        }
        if (tower == null)
        {

            tower = FindAnyObjectByType<TowerController>();
            if (tower == null)
            {
                tower = Instantiate(Resources.Load<GameObject>("Prefabs/Tower")).GetComponent<TowerController>();
            }
            tower.Init();
        }
       
    }
  
    public TowerController tower;
    public PlayerController player;
    public BuildController build;
    public MouseController mouse;
    public MapManager grid;
   
    public LightController lightController;

    public bool isKeepingTower;

    public GameObject weapon;

    public bool isHandleUI;
}
