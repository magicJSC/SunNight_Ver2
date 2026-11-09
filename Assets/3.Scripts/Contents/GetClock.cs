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
            var clock = Managers.Game.timeController;
            clock.gameObject.SetActive(true);
            if (FieldSession.Active && clock.day == 0)
                clock.RestoreClock(1, 330);
            isComplete = true;
        }
    }
}
