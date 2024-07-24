using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class BaseScene : MonoBehaviour
{
    public SceneType SceneType = Define.SceneType.None;
    protected bool _init = false;

    public void Awake()
    {
        Init();
    }

    protected virtual bool Init()
    {
        if (_init)
            return false;

        _init = true;
        Managers.Init();

        return true;
    }

    public virtual void Clear()
    {

    }
}
