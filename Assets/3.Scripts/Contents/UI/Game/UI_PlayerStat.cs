using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_PlayerStat : MonoBehaviour
{
    Image hpImage;
    Image hungerImage;

    PlayerStat playerStat;


    Animator anim;

    public void Start()
    {
        hpImage = Util.FindChild<Image>(gameObject, "HP", true);
        hungerImage = Util.FindChild<Image>(gameObject, "Hunger", true);

        GetComponent<Canvas>().worldCamera = Camera.main;

        anim = GetComponent<Animator>();
        playerStat = GetComponentInParent<PlayerStat>();
        playerStat.hpEvent += SetHpBar;
        playerStat.hungerBarEvent += SetHungerBar;
        playerStat.damagedEvent += GetDamageEffect;
    }

    void GetDamageEffect()
    {
        anim.Play("Damage",-1,0);
    }

    void SetHpBar(Stat stat)
    {
        StartCoroutine(UpdateHPBar(stat));
    }

    IEnumerator UpdateHPBar(Stat stat)
    {
        float hp = playerStat.Hp;
        while (true)
        {
            yield return null;

            if(Mathf.Approximately(hpImage.fillAmount,stat.Hp / stat.maxHP))
                yield break;
            if (hp != playerStat.Hp)
                yield break;
            hpImage.fillAmount = Mathf.Lerp(hpImage.fillAmount, stat.Hp / stat.maxHP, 0.1f);
        }
    }

    void SetHungerBar(float ratio)
    {
        hungerImage.fillAmount = ratio;
    }
}
