using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class CutSceneTrigger : MonoBehaviour
{
    [SerializeField] PlayableDirector director;

    bool finishedPlay;

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (finishedPlay)
            return;
        if(collision.GetComponent<IPlayer>() != null)
        {
            finishedPlay = true;
            director.Play();
        }
    }
}
