using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetClock : MonoBehaviour
{
    bool isComplete;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isComplete)
            return;

        if(collision.GetComponent<PlayerController>() != null)
        {
            Managers.Game.timeController.gameObject.SetActive(true);
            isComplete = true;
        }
    }
}
