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
        StartCoroutine(UpdateHPBar(f));
        anim.Play("Hited",-1,0);
    }

    IEnumerator UpdateHPBar(float ratio)
    {
        while (true)
        {
            yield return null;

            if (Mathf.Approximately(fill.fillAmount, ratio))
                yield break;
            fill.fillAmount = Mathf.Lerp(fill.fillAmount, ratio, 0.1f);
        }
    }

    public override void Init()
    {
        Bind<Image>(typeof(Images));
        fill = GetImage((int)Images.Fill);
        anim = GetComponent<Animator>();

        if(transform.parent.TryGetComponent<Stat>(out var stat))
        {
            stat.Hp = stat.maxHP;
            fill.fillAmount = 1;
            stat.hpEvent += SetHp;
        }
    }
}
