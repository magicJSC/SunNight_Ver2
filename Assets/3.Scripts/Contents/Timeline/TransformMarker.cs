using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TransformMarker : Marker, INotification, INotificationOptionProvider
{
    public string moveTarget;
    public string startTransform;
    public string targetTransform;
    public float playTime;

    [Space(20)]
    [SerializeField] bool retroative = false;
    [SerializeField] bool emitOnce = false;

    public PropertyName id => new PropertyName();

    public NotificationFlags flags => (retroative ? NotificationFlags.Retroactive : default)
        | (emitOnce ? NotificationFlags.TriggerOnce : default);
}
