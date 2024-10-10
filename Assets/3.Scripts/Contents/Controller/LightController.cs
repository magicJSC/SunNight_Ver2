using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightController : MonoBehaviour
{
    Animator anim;

    bool init;
    public void Start()
    {
        Init();
    }

    public void Init()
    {
        anim = GetComponent<Animator>();
    }

    public void SetAction()
    {
        TimeController.nightEvent += SetNight;
        TimeController.morningEvent += SetMorning;
    }

    public void SetNight()
    {
        if(init)
         anim.Play("Night");
        else
        {
            init = true;
            anim.Play("isNight");
        }
    }

    public void SetMorning()
    {
        if (init)
            anim.Play("Morning");
        else
        {
            init = true;
            anim.Play("isMorning");
        }
    }
}
