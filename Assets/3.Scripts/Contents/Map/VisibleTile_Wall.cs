using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class VisibleTile_Wall : MonoBehaviour
{
    Tilemap tile;

    public Tilemap deco;
    private void Start()
    {
        tile = GetComponent<Tilemap>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.TryGetComponent<PlayerController>(out var player))
        {
            DOVirtual.Color(tile.color, new Color(1, 1, 1, 0.3f), 1,(value) =>
            {
                tile.color = value;
            });
            if (deco != null)
            {
                DOVirtual.Color(deco.color, new Color(1, 1, 1, 0.3f), 1, (value) =>
                {
                    deco.color = value;
                });
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<PlayerController>(out var player))
        {
            DOVirtual.Color(tile.color, new Color(1, 1, 1, 1), 1, (value) =>
            {
                tile.color = value;
            });

            if (deco != null)
            {
                DOVirtual.Color(deco.color, new Color(1, 1, 1, 1), 1, (value) =>
                {
                    deco.color = value;
                });
            }
        }
    }
}
