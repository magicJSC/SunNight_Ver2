using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class MapController : MonoBehaviour
{
    public AssetReferenceGameObject mapUIAsset;

    GameObject mapUI;
    private void Start()
    {
        mapUIAsset.LoadAssetAsync().Completed += (obj)=>
        {
            mapUI = Instantiate(obj.Result);
            mapUI.SetActive(false);
        };
    }
}
