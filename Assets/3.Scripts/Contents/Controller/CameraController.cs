using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;

    private void Update()
    {
        transform.position = target.position + new Vector3(0, 0, -10);
    }
}
