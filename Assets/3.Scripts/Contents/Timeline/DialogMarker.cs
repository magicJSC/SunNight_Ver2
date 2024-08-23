using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class DialogMarker : Marker, INotification,INotificationOptionProvider
{
    [SerializeField] string message = "";
    [SerializeField] string nameText = "";
    [SerializeField] float pausePerLetter = 0.1f;
    [SerializeField] AudioClip perLetterSound;

    [Space(20)]
    [SerializeField] bool retroative = false;
    [SerializeField] bool emitOnce = false;

    public PropertyName id => new PropertyName();
    public string Message => message;
    public float PausePerLetter => pausePerLetter;
    public AudioClip PerLetterSound => perLetterSound;
    public string Name => nameText;

    public NotificationFlags flags => (retroative ? NotificationFlags.Retroactive : default)
        | (emitOnce ? NotificationFlags.TriggerOnce : default);
}
