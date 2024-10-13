using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRun : MonoBehaviour
{
    PlayerStat playerStat;

    bool cancel;

    private void Start()
    {
        playerStat = GetComponentInParent<PlayerStat>();
    }

    public void RunAction(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            cancel = false;
            playerStat.Speed *= 1.5f;
            StartCoroutine(ReduceHunger());
        }
        else if (context.canceled)
        {
            playerStat.Speed /= 1.5f;
            cancel = true;
        }
    }

    IEnumerator ReduceHunger()
    {
        while (!cancel)
        {
            if (playerStat.Hunger > 0)
            {
                playerStat.Hunger -= Time.deltaTime / 50;
            }
            else
                break;
            yield return null;
        }
    }
}
