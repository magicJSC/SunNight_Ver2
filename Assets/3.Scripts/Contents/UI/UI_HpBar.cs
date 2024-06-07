using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_HpBar : UI_Base
{
    Image fill;
    Animator anim;

    enum Images
    {
        Fill
    }

    public void SetHp(float f)
    {
        fill.fillAmount = f;
        anim.Play("Hited",-1,0);
    }

    public override void Init()
    {
        Bind<Image>(typeof(Images));
        fill = GetImage((int)Images.Fill);
        anim = GetComponent<Animator>();
    }
}
