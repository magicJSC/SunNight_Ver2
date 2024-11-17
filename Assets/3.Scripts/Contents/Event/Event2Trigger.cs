using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Event2Trigger : MonoBehaviour
{
    public GameObject[] fac;

    public AudioClip clip;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.GetComponent<Obstacle>() != null)
        {
            for(int i = 0; i < fac.Length; i++)
            {
                fac[i].GetComponent<Animator>().enabled = false;
                fac[i].GetComponent<AudioSource>().enabled = false;
            }
            GetComponent<AudioSource>().PlayOneShot(clip);
            StartCoroutine(Clear());
        }
    }

    IEnumerator Clear()
    {
        yield return new WaitForSeconds(1);
        Destroy(gameObject);
    }
}
