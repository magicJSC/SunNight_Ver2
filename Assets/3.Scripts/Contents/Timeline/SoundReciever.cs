using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class SoundReciever : MonoBehaviour, INotificationReceiver
{
    public void OnNotify(Playable origin, INotification notification, object context)
    {
        if (notification is SoundMarker soundMarker)
        {
            if (soundMarker.IsPlayMark)
            {
                Managers.Sound.Play(Define.Sound.Bgm, soundMarker.PlayAudio);
            }
            else
                Managers.Sound.Stop(Define.Sound.Bgm);
        }
    }
}
