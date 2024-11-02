using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class TransformReciever : MonoBehaviour, INotificationReceiver
{
    [SerializeField] TransformAnimator transformAnimator;

    public void OnNotify(Playable origin, INotification notification, object context)
    {
        if (notification is TransformMarker transformMarker)
        {
            transformAnimator.MoveTransformToTarget(
                GetTransform(transformMarker.moveTarget),
               GetTransform(transformMarker.startTransform),
                GetTransform(transformMarker.targetTransform),
                transformMarker.playTime
                );
        }
    }

    Transform GetTransform(string key)
    {
        switch (key)
        {
            case "Player":
                return Managers.Game.player.transform;
            case "MainCamera":
                return Camera.main.transform;
            default:
                return Util.FindChild<Transform>(gameObject, key,true);
        }
    }
}
