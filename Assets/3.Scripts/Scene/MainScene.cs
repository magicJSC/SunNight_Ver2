using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainScene : BaseScene
{
    [SerializeField]
    AudioClip mainSound;
    [SerializeField]
    Texture2D normal;
    protected override bool Init()
    {
        if (base.Init() == false)
            return false;

        Managers.Sound.Play(Define.Sound.Bgm,mainSound);
        Cursor.SetCursor(normal, Vector2.zero, CursorMode.Auto);
        return true;
    }
}
