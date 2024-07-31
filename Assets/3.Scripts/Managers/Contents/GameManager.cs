using System;
using System.Collections;
using System.Collections.Generic;
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

    public bool isplayingTutorial = true;

    public bool BGMOn;
    public bool EffectSoundOn;
}
