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


    static AudioClip sound;
    static GameObject effect;  

    [HideInInspector]
    public Text countText;

    [HideInInspector]
    public int Count { get { return count; } set { count = value; SetCountText(); } }

    int count = 1;

    Rigidbody2D rigid;

    private void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        GetCountText();
        countText.text = $"{count}";
        SetCountText();
        if (effect == null)
        {
            effectAsset.LoadAssetAsync().Completed += (obj) =>
            {
                effect = obj.Result;
            };
        }
        if (soundAsset == null)
        {
            soundAsset.LoadAssetAsync().Completed += (clip) =>
            {
                sound = clip.Result;
            };
        }
        StartCoroutine(StopSelf());
        Destroy(gameObject, 60);
    }

    IEnumerator StopSelf()
    {
        while(true) 
        {
            yield return null;
            if(rigid.velocity != Vector2.zero)
            {
                Vector2.Lerp(rigid.velocity,Vector2.zero,0.1f);
            }
        }
    }
    

    public void DestroyThis()
    {
        if(effect != null)
            Instantiate(effect,transform.position, Quaternion.identity);
        Destroy(gameObject);
        if (sound != null)
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
