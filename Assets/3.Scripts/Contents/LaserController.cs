using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserController : MonoBehaviour
{
    LineRenderer line;

    private void Start()
    {
        line = GetComponent<LineRenderer>();
        StartCoroutine(ShootLaser());
    }

    IEnumerator ShootLaser()
    {
        while (true)
        {
            yield return null;
            line.SetPosition(0, transform.position);
            line.SetPosition(1, Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }
    }
}
