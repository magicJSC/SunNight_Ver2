using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;


public class SoundMarker : Marker, INotification
{
    [SerializeField] bool isPlayMark;
    [SerializeField] bool isStopMark;
    [SerializeField] AudioClip playAudio;

    public PropertyName id => new PropertyName();
    public bool IsPlayMark => isPlayMark;
    public bool IsStopMark => isStopMark;
    public AudioClip PlayAudio => playAudio;
}
