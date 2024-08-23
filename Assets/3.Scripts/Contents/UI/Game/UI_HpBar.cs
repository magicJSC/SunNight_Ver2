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

    public void SetHp(Stat stat)
    {
        StartCoroutine(UpdateHPBar(stat));
        anim.Play("Hited", -1, 0);
    }

    IEnumerator UpdateHPBar(Stat stat)
    {
        float hp = stat.Hp;
        while (true)
        {
            yield return null;

            if (Mathf.Approximately(fill.fillAmount, stat.Hp / stat.maxHP))
                yield break;
            if(hp != stat.Hp)
                yield break;
            fill.fillAmount = Mathf.Lerp(fill.fillAmount, stat.Hp / stat.maxHP, 0.1f);
        }
    }
}
