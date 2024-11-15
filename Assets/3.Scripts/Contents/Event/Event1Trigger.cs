using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class Event1Trigger : MonoBehaviour
{
    bool playedEvent;

    public PlayableDirector beforeEventCut;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (playedEvent)
            return;
        if(collision.GetComponent<TowerController>() != null)
        {
            playedEvent = true;
            Managers.Game.canMoveTower = false;
            beforeEventCut.Play();
        }


    }
}
