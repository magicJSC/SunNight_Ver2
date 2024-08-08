using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager
{
    private AudioSource[] _audioSources = new AudioSource[(int)Define.Sound.Max];

    private GameObject _soundRoot = null;

    public void Init()
    {
        if (_soundRoot == null)
        {
            _soundRoot = GameObject.Find("@SoundRoot");
            if (_soundRoot == null)
            {
                _soundRoot = new GameObject { name = "@SoundRoot" };
                UnityEngine.Object.DontDestroyOnLoad(_soundRoot);

                string[] soundTypeNames = System.Enum.GetNames(typeof(Define.Sound));
                for (int count = 0; count < soundTypeNames.Length - 1; count++)
                {
                    GameObject go = new GameObject { name = soundTypeNames[count] };
                    _audioSources[count] = go.AddComponent<AudioSource>();
                    go.transform.parent = _soundRoot.transform;
                }

                _audioSources[(int)Define.Sound.Bgm].loop = true;
                _audioSources[(int)Define.Sound.SubBgm].loop = true;

                effectVolume = 0.5f;
                bgmVolume = 0.5f;
            }
        }
    }

    public void Play(Define.Sound type)
    {
        AudioSource audioSource = _audioSources[(int)type];
        audioSource.Play();
    }



    public void Play(Define.Sound type, AudioClip audioClip, float pitch = 1.0f)
    {
        AudioSource audioSource = _audioSources[(int)type];

        if (type == Define.Sound.Bgm)
        {
            if (audioSource.isPlaying)
                audioSource.Stop();
            if (audioSource.clip == audioClip)
                return;
            audioSource.clip = audioClip;
            audioSource.Play();
        }
        else if (type == Define.Sound.SubBgm)
        {
            if (audioSource.isPlaying)
                audioSource.Stop();

            audioSource.clip = audioClip;
            audioSource.Play();
        }
        else
        {
            audioSource.pitch = pitch;
            audioSource.PlayOneShot(audioClip);
        }
    }

    public Action<float> effectVolumeEvent;
    public Action<float> bgmVolumeEvent;

    public float EffectVolume
    {
        get { return effectVolume; }
        set
        {
            effectVolume = value;
            effectVolumeEvent.Invoke(value);
            SetVolume(Define.Sound.Effect, value);
        }
    }
    float effectVolume = 0.5f;

    public float BgmVolume
    {
        get { return bgmVolume; }
        set
        {
            bgmVolume = value;
            bgmVolumeEvent.Invoke(value);
            SetVolume(Define.Sound.Bgm, value);
        }
    }
    float bgmVolume = 0.5f;

    public void Stop(Define.Sound type)
    {
        AudioSource audioSource = _audioSources[(int)type];
        audioSource.Stop();
    }

    public void SetVolume(Define.Sound type, float ratio)
    {
        AudioSource audioSource = _audioSources[(int)type];
        audioSource.volume = ratio;
    }
}
