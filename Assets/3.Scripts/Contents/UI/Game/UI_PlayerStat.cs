using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_PlayerStat : UI_Base
{
    Image hpImage;
    Image energyImage;
    Image hungerImage;
    GameObject back;

    PlayerStat playerStat;

    enum GameObjects
    {
        HP,
        Energy,
        Hunger,
        Background
    }

    Animator anim;

    public override void Init()
    {
        Bind<GameObject>(typeof(GameObjects));
        hpImage = Get<GameObject>((int)GameObjects.HP).GetComponent<Image>();
        energyImage = Get<GameObject>((int)GameObjects.Energy).GetComponent<Image>();
        hungerImage = Get<GameObject>((int)GameObjects.Hunger).GetComponent<Image>();
        back = Get<GameObject>((int)GameObjects.Background);

        anim = GetComponent<Animator>();
        playerStat = Managers.Game.player.GetComponent<PlayerStat>();
        playerStat.hpBarEvent += SetHpBar;
        playerStat.energyBarEvent += SetEnergyBar;
        playerStat.hungerBarEvent += SetHungerBar;
        playerStat.damageEvent += GetDamageEffect;

        playerStat.Hp = playerStat.maxHP;
        playerStat.Energy = playerStat.maxEnergy;
        playerStat.Hunger = playerStat.maxHunger;

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


    void SetEnergyBar(float ratio)
    {
        energyImage.fillAmount = ratio;
    }

    void SetHungerBar(float ratio)
    {
        hungerImage.fillAmount = ratio;
    }
}
