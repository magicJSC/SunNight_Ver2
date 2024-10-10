using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class TowerPosUnLock : MonoBehaviour
{
    public static Action unlockEvent;

    Animator anim;
    bool unlocked;
    private void Start()
    {
        anim = GetComponent<Animator>();
        //if (Managers.Game.isUnlockTowerPos[index])
          //  anim.Play("Unlock");
    }

    [SerializeField] int index;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //if (Managers.Game.isUnlockTowerPos[index])
        //   return;
        if (unlocked)
            return;
        if(collision.GetComponent<IPlayer>() != null)
        {
            if (!Managers.Game.completeTutorial)
                unlockEvent?.Invoke();

            Unlock();
        }
    }
    public void Unlock()
    {
        unlocked = true;
        Managers.Game.isUnlockTowerPos[index] = true;
        anim.Play("Unlock");
    }
}
