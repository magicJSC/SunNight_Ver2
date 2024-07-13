using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBGM : MonoBehaviour
{
    AudioSource audioSource;
    public AudioClip morningAudio;
    public AudioClip nightAudio;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        TimeController.morningEvent += PlayMorningBGM;
        TimeController.nightEvent += PlayNightBGM;
    }

    void PlayMorningBGM()
    {
        audioSource.clip = morningAudio;
        audioSource.Play();
    }

    void PlayNightBGM()
    {
        audioSource.clip = nightAudio;
        audioSource.Play();
    }
}
