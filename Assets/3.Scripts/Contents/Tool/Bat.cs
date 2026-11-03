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
       // Death can synchronously remove contacts. Iterate a stable, distinct swing snapshot.
       foreach(var target in new HashSet<Transform>(damageList))
        {
            if(target==null) continue;
            Vector3 hitPosition=target.position;
            target.GetComponent<IGetPlayerDamage>().GetDamage(FieldSession.Active ? (FieldSession.State.Has("U1") ? 9 : 6) : _damage);
            if(effect!=null) Instantiate(effect,hitPosition,Quaternion.identity);
            if (target!=null && target.gameObject.activeInHierarchy &&
                (!target.TryGetComponent<Stat>(out var targetStat) || targetStat.Hp>0) &&
                target.TryGetComponent<IKnockBack>(out var knockBack))
            {
                knockBack.StartKnockBack((point - transform.position).normalized);
            }
        }
        damageList.RemoveAll(target=>target==null);
       
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
