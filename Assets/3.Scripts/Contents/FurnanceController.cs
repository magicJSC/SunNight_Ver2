using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FurnanceController : MonoBehaviour, ICaninteract
{
    public bool isConnected { get; set; }

    public GameObject canInteractSign { get; set; }
    public bool canInteract { get; set; }

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
        smeltUI.isSmelting = true;
        StartCoroutine(SmeltItem());
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
        smeltUI.isSmelting = false;
        _smeltCurTime = 0;
        SetTimer();
    }

    public void SetTimer()
    {
        smeltUI.timer.fillAmount = _smeltCurTime / smeltTime;
    }

    public void Interact()
    {
        if (!canInteract || smeltUI.gameObject.activeSelf)
            return;
        smeltUI.gameObject.SetActive(true);
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.TryGetComponent<PlayerController>(out PlayerController player))
        {
            canInteract = true;
            canInteractSign.SetActive(canInteract);
            player.interact = Interact;
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<PlayerController>(out PlayerController player))
        {
            canInteract = false;
            canInteractSign.SetActive(canInteract);
        }
    }
}
