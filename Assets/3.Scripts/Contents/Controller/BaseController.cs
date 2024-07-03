using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseController : MonoBehaviour
{
    protected abstract void Init();

    private void Start()
    {
        Init();
    }
}
