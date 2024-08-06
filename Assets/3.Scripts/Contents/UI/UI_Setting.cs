using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Setting : UI_Base
{
    bool init;

    [SerializeField]
    Sprite effectOn;
    [SerializeField]
    Sprite effectOff;

    [SerializeField]
    Sprite bgmOn;
    [SerializeField]
    Sprite bgmOff;

    Image effectIcon;
    Image bgmIcon;

    Text effectRatioText;
    Text bgmRatioText;

    Slider effectSlider;
    Slider bgmSlider;

    GameObject close;

    public override void Init()
    {
        init = true;

        effectIcon = Util.FindChild(gameObject, "EffectIcon",true).GetComponent<Image>();
        bgmIcon = Util.FindChild(gameObject, "BGMIcon",true).GetComponent<Image>();
        effectRatioText = Util.FindChild(gameObject,"EffectRatio",true).GetComponent<Text>();
        bgmRatioText = Util.FindChild(gameObject, "BGMRatio", true).GetComponent<Text>();
        bgmSlider = Util.FindChild(gameObject, "BGM", true).GetComponent<Slider>();
        effectSlider = Util.FindChild(gameObject, "Effect", true).GetComponent<Slider>();

        close = Util.FindChild(gameObject, "Close", true);

        close.GetComponent<UI_EventHandler>()._OnClick += Close;

        Managers.Sound.bgmVolumeEvent += SetBgmSoundUI;
        Managers.Sound.effectVolumeEvent += SetEffectSoundUI;

        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (!init)
            return;
        effectSlider.value = Managers.Sound.EffectVolume;
        bgmSlider.value = Managers.Sound.BgmVolume;
    }

    void Close(PointerEventData p)
    {
        gameObject.SetActive(false);
    }

    public void GetBgmVolume()
    {
        Managers.Sound.BgmVolume = bgmSlider.value;
    }

    public void GetEffectVolume()
    {
        Managers.Sound.EffectVolume = effectSlider.value;
    }

    void SetEffectSoundUI(float ratio)
    {
        if(ratio == 0)
            effectIcon.sprite = effectOff;
        else
            effectIcon.sprite = effectOn;

        effectRatioText.text = $"{Mathf.Floor(ratio * 100)}";
    }

    void SetBgmSoundUI(float ratio)
    {
        if (ratio == 0)
            bgmIcon.sprite = bgmOff;
        else
            bgmIcon.sprite = bgmOn;

        bgmRatioText.text = $"{Mathf.Floor(ratio * 100)}";
    }
}
