using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Help : MonoBehaviour
{
    RectTransform help;
    private void Start()
    {
        help = Util.FindChild<RectTransform>(gameObject, "Help", true);
        help.localScale = new Vector2(0, 1);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.GetComponent<PlayerController>() != null)
        {
            help.transform.DOScaleX(1, 0.5f);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerController>() != null)
        {
            help.transform.DOScaleX(0, 0.5f);
        }
    }
}
