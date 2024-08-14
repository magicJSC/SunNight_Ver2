using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    public void Shake(float duration,float strength)
    {
        transform.DOShakePosition(duration, strength);
    }
}
