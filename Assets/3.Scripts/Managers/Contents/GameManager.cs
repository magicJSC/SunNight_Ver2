using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Define;

public class GameManager : MonoBehaviour
{
    #region 게임데이터
    //타워에 설치된 건축물 위치 데이터
    public Dictionary<Vector2, int> buildData = new Dictionary<Vector2, int>();
    #endregion
    public void Init()
    {
        if(grid == null)
        {
            grid = FindAnyObjectByType<GridManager>();
            if(grid == null)
            {
                grid = Instantiate(Resources.Load<GameObject>("Prefabs/Grid")).GetComponent<GridManager>();
            }
            grid.Init();
        }
        if (mouse == null)
        {
            mouse = FindAnyObjectByType<MouseController>();
            if (mouse == null)
            {
                mouse = Instantiate(Resources.Load<GameObject>("Prefabs/Mouse")).GetComponent<MouseController>();
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
        if (player == null)
        {
            player = FindAnyObjectByType<PlayerController>();
            if (player == null)
            {
                player = Instantiate(Resources.Load<GameObject>("Prefabs/Player")).GetComponent<PlayerController>();
            }
            player.Init();
        }
        if (lights == null)
        {
            lights = FindAnyObjectByType<LightController>();
            if(lights == null)
            {
                lights = Instantiate(Resources.Load<GameObject>("Prefabs/Light")).GetComponent<LightController>();
            }
            lights.Init();
        }

        Managers.Inven.Set_HotBar_Choice();
    }
   
    public void OnUpdate()
    {
        SetTime();
    }

    public TowerController tower;
    public PlayerController player;
    public BuildController build;
    public MouseController mouse;
    public GridManager grid;
   

   
    public TimeType timeType = TimeType.Morning;
    public LightController lights;
    public float curTime = 0;
    public float hour = 6;
    public float minute = 0;

    void SetTime()
    {
        if (curTime >= 1)
        {
            curTime = 0;
            lights.SetLight();

            minute++;
            if (minute == 60)
            {
                minute = 0;
                hour++;
                if (hour == 6)
                    timeType = TimeType.Morning;
                else if (hour == 18)
                {
                    timeType = TimeType.Night;

                    if (Managers.Inven.hotBar_itemInfo[Managers.Inven.hotBar_itemInfo.Length-1].keyType == KeyType.Exist)
                     build.BuildTower(true);
                }
                if (hour == 24)
                    hour = 0;
            }
        }
        else
            curTime += Time.deltaTime;
    }
}
