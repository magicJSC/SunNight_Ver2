using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseEffect : MonoBehaviour
{
    public void Disappear()
    {
        Destroy(gameObject);
    }
}
