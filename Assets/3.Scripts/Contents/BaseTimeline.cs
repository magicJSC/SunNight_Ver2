using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseTimeline : MonoBehaviour
{
    public void StartTimeline()
    {
        
        Time.timeScale = 0;
    }

    public void EndTimeline()
    {
        Time.timeScale = 1;
        Destroy(gameObject);
    }
}
