using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    public static Action interactCheckerEvent;

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
        canInteractObj = null;
        for (int i = 0; i < interactObjectList.Count; i++)
        {
            if (canInteractObj == null)
                canInteractObj = interactObjectList[i];
            else if ((canInteractObj.transform.position - transform.position).magnitude > (interactObjectList[i].transform.position- transform.position).magnitude)
                canInteractObj = interactObjectList[i];

            interactObjectList[i].GetComponent<IInteractObject>().HideInteractSign();
        }
        if (canInteractObj != null)
        {
            interactAction = null;
            cancelAction = null;
            canInteractObj.GetComponent<IInteractObject>().ShowInteractSign();
            canInteractObj.GetComponent<IInteractObject>().SetAction(this);
        }
    }

    
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<IInteractObject>(out var interact))
        {
            interactCheckerEvent?.Invoke();
            interactObjectList.Add(collision.gameObject);
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<IInteractObject>(out var interact))
        {
            interactObjectList.Remove(collision.gameObject);
            interact.HideInteractSign();
        }
    }
}
