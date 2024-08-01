using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanInteractChecker : MonoBehaviour,ICheckCanInteract
{
    
    IInteractObject interactObject;

    void Start()
    {
        interactObject = transform.GetComponentInParent<IInteractObject>();
    }

    public void EnterPlayer(PlayerController player)
    {
        player.interactObjectList.Add(transform.parent.gameObject);
        player.SetInteractObj();
    }

    public void ExitPlayer(PlayerController player)
    {
        player.interactObjectList.Remove(transform.parent.gameObject);
        interactObject.canInteractSign.SetActive(false);
        player.SetInteractObj();
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<PlayerController>(out var player))
        {
            EnterPlayer(player);
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<PlayerController>(out var player))
        {
            ExitPlayer(player);
        }
    }
}
