using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    [HideInInspector]
    public List<GameObject> interactObjectList = new List<GameObject>();
    GameObject canInteractObj;

    public Action interactAction;
    public Action cancelAction;

    private void OnEnable()
    {
        StartCoroutine(UpdateInteract());
    }

    IEnumerator UpdateInteract()
    {
        while (true)
        {
            yield return null;
            SetInteractObj();
        }
    }

    public void InteractAction(InputAction.CallbackContext context)
    {
        if (Time.timeScale == 0)
            return;

        if (context.performed)
        {
            interactAction?.Invoke();
        }
        else if (context.canceled)
        {
            cancelAction?.Invoke();
        }
        
    }

    public void SetInteractObj()
    {
        interactObjectList.RemoveAll(obj => obj == null || !obj.activeInHierarchy);
        GameObject nearest = null;
        foreach (var candidate in interactObjectList)
        {
            if (nearest == null || (candidate.transform.position - transform.position).sqrMagnitude <
                (nearest.transform.position - transform.position).sqrMagnitude)
                nearest = candidate;
        }
        if (ReferenceEquals(nearest, canInteractObj)) return;

        if (canInteractObj != null) cancelAction?.Invoke();
        if (canInteractObj != null)
        {
            var previous = canInteractObj.GetComponent<IInteractObject>();
            previous.CancelAction(this);
            previous.HideInteractSign();
        }
        canInteractObj = nearest;
        interactAction = null;
        cancelAction = null;
        if (canInteractObj != null)
        {
            var current = canInteractObj.GetComponent<IInteractObject>();
            current.ShowInteractSign();
            current.SetAction(this);
        }
    }

    
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<IInteractObject>(out var interact))
        {
            if (!interactObjectList.Contains(collision.gameObject))
                interactObjectList.Add(collision.gameObject);
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<IInteractObject>(out var interact))
        {
            interactObjectList.Remove(collision.gameObject);
            SetInteractObj();
        }
    }
}
