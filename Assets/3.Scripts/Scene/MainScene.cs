using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainScene : BaseScene
{
    [SerializeField]
    AudioClip mainSound;

    protected override bool Init()
    {
        if (base.Init() == false)
            return false;

        Managers.Sound.Play(Define.Sound.Bgm,mainSound);
        return true;
    }
}
