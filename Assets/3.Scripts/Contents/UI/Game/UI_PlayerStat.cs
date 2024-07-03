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

    public override void Init()
    {
        Bind<GameObject>(typeof(GameObjects));
        hpImage = Get<GameObject>((int)GameObjects.HP).GetComponent<Image>();
        energyImage = Get<GameObject>((int)GameObjects.Energy).GetComponent<Image>();
        hungerImage = Get<GameObject>((int)GameObjects.Hunger).GetComponent<Image>();
        back = Get<GameObject>((int)GameObjects.Background);

        UI_EventHandler evt = back.GetComponent<UI_EventHandler>();
        evt._OnEnter += (PointerEventData p) => { Managers.Game.isHandleUI = true; };
        evt._OnExit += (PointerEventData p) => { Managers.Game.isHandleUI = false; };

        playerStat = Managers.Game.player.GetComponent<PlayerStat>();
        playerStat.hpBarEvent += SetHpBar;
        playerStat.energyBarEvent += SetEnergyBar;
        playerStat.hungerBarEvent += SetHungerBar;
    }

    void SetHpBar(float ratio)
    {
        hpImage.fillAmount = ratio;
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
