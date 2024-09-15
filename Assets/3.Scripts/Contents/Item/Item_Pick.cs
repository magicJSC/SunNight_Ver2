using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class Item_Pick : Item
{
    public static Action tutorialEvent;

    public AssetReferenceGameObject effectAsset;
    public AssetReferenceT<AudioClip> soundAsset;


    AudioClip sound;
    GameObject effect;  

    [HideInInspector]
    public Text countText;

    [HideInInspector]
    public int Count { get { return count; } set { count = value; SetCountText(); } }

    int count = 1;

    private void Start()
    {
        GetCountText();
        countText.text = $"{count}";
        SetCountText();
        effectAsset.LoadAssetAsync().Completed += (obj) =>
        {
            effect = obj.Result;
        };
        soundAsset.LoadAssetAsync().Completed += (clip) =>
        {
            sound = clip.Result;
        };
    }

    public void DestroyThis()
    {
        Instantiate(effect,transform.position, Quaternion.identity);
        Managers.Sound.Play(Define.Sound.Effect, sound);
    }

    void SetCountText()
    {
        if (countText == null)
            GetCountText();

        if(count != 1)
            countText.text = $"{count}";
        else
            countText.text = $"";
    }

    void GetCountText()
    {
        countText = Util.FindChild(gameObject, "Count", true).GetComponent<Text>();
    }
}
