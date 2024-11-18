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
        Collider2D[] cols = Physics2D.OverlapBoxAll(transform.GetChild(0).position + (point - transform.position).normalized / 2, size, angle);
        foreach (Collider2D col in cols)
        {
            if (col.TryGetComponent<IGetMonsterDamage>(out var player))
                return;

            if (col.TryGetComponent<IGetPlayerDamage>(out var monster))
            {
                monster.GetDamage(_damage);
                Instantiate(effect, col.transform.position, Quaternion.identity);
            }
            if (col.TryGetComponent<IKnockBack>(out var knockBack))
            {
                knockBack.StartKnockBack(transform);
            }
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.GetChild(0).position, size);
    }
}
