using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightController : MonoBehaviour
{
    Light2D lights;

    Animator anim;

    public void Init()
    {
        lights = GetComponent<Light2D>();
        lights.color = Color.white;
        anim = GetComponent<Animator>();
        TimeController.nightEvent += SetNight;
        TimeController.morningEvent += SetMorning;
    }

    public void SetNight()
    {
        anim.Play("Night");
    }

    public void SetMorning()
    {
        anim.Play("Morning");
    }
}
