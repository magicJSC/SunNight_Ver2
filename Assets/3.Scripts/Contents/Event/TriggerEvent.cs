using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerEvent : MonoBehaviour
{
    public Action triggerEvent;
    public bool once;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.GetComponent<PlayerController>() != null)
        {
            triggerEvent?.Invoke();
            if(once)
                Destroy(gameObject);
        }
    }
}
