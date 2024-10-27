using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class Sword : ToolController
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
        Managers.Sound.Play(Define.Sound.Effect, sound);
        Collider2D[] cols = Physics2D.OverlapBoxAll(transform.GetChild(0).position + (point - transform.position).normalized, size, angle);
        foreach (Collider2D col in cols)
        {
            if (col.TryGetComponent<IPlayer>(out var player) || col.TryGetComponent<IBuilding>(out var build))
                return;

            if (col.TryGetComponent<IMonster>(out var monster))
            {
                monster.GetDamage(_damage);
                Instantiate(effect, col.transform.position, Quaternion.identity);
            }
            else if(col.TryGetComponent<IGetDamage>(out var get))
            {
                get.GetDamage(_damage);
                Instantiate(effect, col.transform.position, Quaternion.identity);
            }
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.GetChild(0).position, size);
    }
}
