using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FurnanceController : MonoBehaviour, IInteractObject
{

    public GameObject canInteractSign { get; set; }

    UI_Smelt smeltUI;

    public float smeltTime;
    float _smeltCurTime=0;

    public void Start()
    {
        smeltUI = Managers.Inven.smeltUI;
        smeltUI.furnanace = this;
        canInteractSign = Util.FindChild(gameObject, "Sign",true);
        canInteractSign.SetActive(false);
    }

    public void StartSmelt()
    {
        UI_Smelt.isSmelting = true;
        StartCoroutine(SmeltItem());
    }

    public void CancelSmelt()
    {
        UI_Smelt.isSmelting = false;
        StopCoroutine(SmeltItem());
        _smeltCurTime = 0;
        SetTimer();
    }

    IEnumerator SmeltItem()
    {
        while(_smeltCurTime < smeltTime)
        {
            _smeltCurTime++;
            yield return new WaitForSeconds(1);
            SetTimer();
        }
        smeltUI.FinishSmelt();
        UI_Smelt.isSmelting = false;
        _smeltCurTime = 0;
        SetTimer();
    }

    public void SetTimer()
    {
        smeltUI.timer.fillAmount = _smeltCurTime / smeltTime;
    }

    public void Interact()
    {
        if (Managers.Game.isKeepingTower)
            return;
        if (smeltUI.gameObject.activeSelf)
            return;
        smeltUI.gameObject.SetActive(true);
    }
}
