using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightController : MonoBehaviour
{
    Light2D lights;

    Color night;
   
    float colorTime =0;
    public void Init()
    {
        lights = GetComponent<Light2D>();
        lights.color = Color.white;
        night = new Color(0.3f, 0.3f, 0.6f);
    }

    public void SetLight()
    {
        if(Managers.Game.timeType == Define.TimeType.Morning)
            colorTime++;
        else
            colorTime--;

        lights.color = new Color(1 - ((1 - night.r) / 12 / 60 * colorTime), 1 - ((1 - night.g) / 12 / 60 * colorTime), 1 - ((1 - night.b) / 12 /60 * colorTime));
    }
}
