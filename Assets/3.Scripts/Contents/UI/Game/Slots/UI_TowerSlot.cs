using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_TowerSlot : UI_Base
{
    Image icon;

    public AssetReferenceT<AudioClip> getTowerSoundAsset;

    AudioClip getTowerSound;

    public new void Init()
    {
        icon = Util.FindChild(gameObject,"Icon",true).GetComponent<Image>();
        GetComponent<Image>().color = Color.yellow;
        icon.sprite = Managers.Game.tower.GetComponent<SpriteRenderer>().sprite;
        HideTowerIcon();

        UI_EventHandler evt = GetComponent<UI_EventHandler>();
        evt._OnEnter += (PointerEventData p) =>
        {
            if (Managers.Game.mouse.CursorType == Define.CursorType.Drag)
            {
                StorageManager.canAbandon = false;
            }
        };
        evt._OnExit += (PointerEventData p) =>
        {
            if (Managers.Game.mouse.CursorType == Define.CursorType.Drag)
            {
                StorageManager.canAbandon = true;
            }
        };

        getTowerSoundAsset.LoadAssetAsync().Completed += (clip) =>
        {
            getTowerSound = clip.Result;
        };
    }

    public void ShowTowerIcon()
    {
        icon.gameObject.SetActive(true);
        Managers.Sound.Play(Define.Sound.Effect,getTowerSound);
    }

    public void HideTowerIcon()
    {
        icon.gameObject.SetActive(false);
    }
}
