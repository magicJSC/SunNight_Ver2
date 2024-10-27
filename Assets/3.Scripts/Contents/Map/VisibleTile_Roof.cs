using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class VisibleTile_Roof : MonoBehaviour
{
    public Tilemap roof;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<IPlayer>(out var player))
        {
            DOVirtual.Color(roof.color, new Color(1, 1, 1, 0), 1, (value) =>
            {
                roof.color = value;
            });
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<IPlayer>(out var player))
        {
            DOVirtual.Color(roof.color, new Color(1, 1, 1, 1), 1, (value) =>
            {
                roof.color = value;
            });
        }
    }
}
