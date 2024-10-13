using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class Journy : MonoBehaviour, IInteractObject
{
    public GameObject canInteractSign { get; set; }
    GameObject changeScene;
   public AssetReferenceGameObject journyUIAsset;

    GameObject journyUI;
    void Start()
    {
        canInteractSign = Util.FindChild(gameObject, "Sign", true);
        changeScene = Util.FindChild(gameObject, "ChangeScene", true);
        journyUIAsset.LoadAssetAsync().Completed += (obj) =>
        {
            journyUI = obj.Result;
        };
        canInteractSign.SetActive(false);
        changeScene.SetActive(false);
    }

    public void HideInteractSign()
    {
        canInteractSign.SetActive(false);
    }

    public void Interact()
    {
       Instantiate(journyUI);
        changeScene.SetActive(true);
    }

    public void ShowInteractSign()
    {
        canInteractSign.SetActive(true);
    }

    public void SetAction(PlayerInteract playerInteract)
    {
        playerInteract.interactAction += Interact;
    }

    public void CancelAction(PlayerInteract playerInteract)
    {
        playerInteract.interactAction -= Interact;
    }
}
