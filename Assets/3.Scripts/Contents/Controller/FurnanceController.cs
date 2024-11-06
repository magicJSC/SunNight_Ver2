using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class FurnanceController : MonoBehaviour, IInteractObject
{

    public GameObject canInteractSign { get; set; }


    public AssetReferenceGameObject smeltUIAsset;

    UI_Smelt smeltUI;

    public float smeltTime;
    float _smeltCurTime=0;

    public void Start()
    {
        smeltUIAsset.LoadAssetAsync().Completed += (obj)=>
        {
            smeltUI = Managers.UI.ShowInvenUI<UI_Smelt>(obj.Result);
            smeltUI.furnanace = this;
        };
        canInteractSign = Util.FindChild(gameObject, "Sign",true);
        canInteractSign.SetActive(false);
    }

    public void StartSmelt()
    {
        smeltUI.isSmelting = true;
        StartCoroutine(SmeltItem());
    }

    public void CancelSmelt()
    {
        smeltUI.isSmelting = false;
        StopCoroutine(SmeltItem());
        _smeltCurTime = 0;
        SetTimer();
    }

    IEnumerator SmeltItem()
    {
        while(_smeltCurTime < smeltTime)
        {
            yield return null;
            _smeltCurTime += Time.deltaTime;
            SetTimer();
        }
        _smeltCurTime = 0;
        SetTimer();
        smeltUI.FinishSmelt();
    }

    public void SetTimer()
    {
        smeltUI.timer.fillAmount = _smeltCurTime / smeltTime;
    }

    public void Interact()
    {
        if (Managers.Game.isMovingTower)
            return;
        if (smeltUI.gameObject.activeSelf)
            return;
        smeltUI.gameObject.SetActive(true);
    }

    public void ShowInteractSign()
    {
        canInteractSign.SetActive(true);
    }

    public void HideInteractSign()
    {
        canInteractSign.SetActive(false);
    }
    public void SetAction(PlayerInteract playerInteract)
    {
        playerInteract.interactAction += Interact;
    }

    void IInteractObject.CancelAction(PlayerInteract playerInteract)
    {
        playerInteract.interactAction -= Interact;
    }
}
