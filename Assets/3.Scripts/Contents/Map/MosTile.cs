using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class MosTile : MonoBehaviour
{
    public AssetReferenceGameObject mosBuffAsset;

    GameObject mosBuffPrefab;
    GameObject mosBuff;

    private void Start()
    {
        mosBuffAsset.LoadAssetAsync().Completed += (obj) =>
        {
            mosBuffPrefab = obj.Result;
        };
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<IBuffReciever>(out var reciever))
        {
            mosBuff = Instantiate(mosBuffPrefab, collision.transform); 
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<IBuffReciever>(out var reciever))
        {
            mosBuff.SetActive(false);
        }
    }
}
