using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_PlayerStat : UI_Base
{
    Image hpImage;
    Image hungerImage;

    PlayerStat playerStat;


    Animator anim;

    public override void Init()
    {
        hpImage = Util.FindChild<Image>(gameObject, "HP", true);
        hungerImage = Util.FindChild<Image>(gameObject, "Hunger", true);

        GetComponent<Canvas>().worldCamera = Camera.main;

        anim = GetComponent<Animator>();
        playerStat = GetComponentInParent<PlayerStat>();
        playerStat.hpBarEvent += SetHpBar;
        playerStat.hungerBarEvent += SetHungerBar;
        playerStat.damageEvent += GetDamageEffect;

        playerStat.Hp = playerStat.maxHP;
    }

    void GetDamageEffect()
    {
        anim.Play("Damage",-1,0);
    }

    void SetHpBar(float ratio)
    {
        StartCoroutine(UpdateHPBar(ratio));
    }

    IEnumerator UpdateHPBar(float ratio)
    {
        float hp = playerStat.Hp;
        while (true)
        {
            yield return null;

            if(Mathf.Approximately(hpImage.fillAmount,ratio))
                yield break;
            if (hp != playerStat.Hp)
                yield break;
            hpImage.fillAmount = Mathf.Lerp(hpImage.fillAmount, ratio,0.1f);
        }
    }

    void SetHungerBar(float ratio)
    {
        hungerImage.fillAmount = ratio;
    }
}
