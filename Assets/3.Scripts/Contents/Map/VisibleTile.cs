using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class VisibleTile : MonoBehaviour
{
    Tilemap tile;
    private void Start()
    {
        tile = GetComponent<Tilemap>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.TryGetComponent<IPlayer>(out var player))
        {
            DOVirtual.Color(tile.color, new Color(1, 1, 1, 0.35f), 1,(value) =>
            {
                tile.color = value;
            });
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<IPlayer>(out var player))
        {
            DOVirtual.Color(tile.color, new Color(1, 1, 1, 1), 1, (value) =>
            {
                tile.color = value;
            });
        }
    }
}
