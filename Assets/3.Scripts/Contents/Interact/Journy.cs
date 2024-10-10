using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class Journy : MonoBehaviour, IInteractObject
{
    public GameObject canInteractSign { get; set; }

   public AssetReferenceGameObject journyUIAsset;

    GameObject journyUI;
    void Start()
    {
        canInteractSign = Util.FindChild(gameObject, "Sign", true);
        journyUIAsset.LoadAssetAsync().Completed += (obj) =>
        {
            journyUI = obj.Result;
        };
    }

    public void HideInteractSign()
    {
        canInteractSign.SetActive(false);
    }

    public void Interact()
    {
       Instantiate(journyUI);
    }

    public void ShowInteractSign()
    {
        canInteractSign.SetActive(true);
    }
}
