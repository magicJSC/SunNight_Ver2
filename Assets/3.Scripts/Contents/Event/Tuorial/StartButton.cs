using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartButton : MonoBehaviour, IInteractObject
{
    public GameObject canInteractSign { get; set; }

    public Action pushAction;

    [HideInInspector]
    public bool isReady;

    void Start()
    {
        canInteractSign = Util.FindChild(gameObject, "Sign", true);
        canInteractSign.SetActive(false);
    }

    public void CancelAction(PlayerInteract playerInteract)
    {
        playerInteract.interactAction -= Interact;
    }

    public void HideInteractSign()
    {
        canInteractSign.SetActive(false);
    }

    public void SetAction(PlayerInteract playerInteract)
    {
        playerInteract.interactAction += Interact;
    }

    public void ShowInteractSign()
    {
        canInteractSign.SetActive(true);
    }

    void Interact()
    {
        if (!isReady)
            return;
        pushAction.Invoke();
        Destroy(gameObject);
    }
}
