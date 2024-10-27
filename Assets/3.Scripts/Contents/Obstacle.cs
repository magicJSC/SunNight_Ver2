using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    private void Start()
    {
        GetComponent<Stat>().dieEvent += Die;
    }

    void Die()
    {
        Destroy(gameObject);
    }
}
