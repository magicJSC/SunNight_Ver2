using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class Bat : ToolController
{
    [SerializeField]
    Vector2 size;

    public AssetReferenceGameObject effectAsset;
    public AssetReferenceT<AudioClip> soundAsset;

    GameObject effect;
    AudioClip sound;

    List<Transform> damageList = new List<Transform>();


    protected override void Init()
    {
        base.Init();
        effectAsset.LoadAssetAsync().Completed += (obj) =>
        {
            effect = obj.Result;
        };
        soundAsset.LoadAssetAsync().Completed += (clip) =>
        {
            sound = clip.Result;
        };
    }

    void Attack()
    {
        if (Managers.Game.isCantPlay)
            return;
        Managers.Sound.Play(Define.Sound.Effect, sound);
       for(int i = 0;i< damageList.Count; i++) 
        {
            if (damageList[i] == null)
            {
                damageList.RemoveAt(i);
                i--;
                continue;
            }

            damageList[i].GetComponent<IGetPlayerDamage>().GetDamage(_damage);
            Instantiate(effect, damageList[i].transform.position, Quaternion.identity);
            if (damageList[i].TryGetComponent<IKnockBack>(out var knockBack))
            {
                knockBack.StartKnockBack((point - transform.position).normalized);
            }
        }
       
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.GetComponent<IGetPlayerDamage>() != null)
        {
            damageList.Add(collision.transform);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<IGetPlayerDamage>() != null)
        {
            damageList.Remove(collision.transform);
        }
    }
}
