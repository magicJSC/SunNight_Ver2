using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class VisibleTile_Wall : MonoBehaviour
{
    Tilemap tile;
    TilemapRenderer tileRender;

    public Tilemap deco;
    public TilemapRenderer decoRender;
    private void Start()
    {
        tile = GetComponent<Tilemap>();
        tileRender = GetComponent<TilemapRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.TryGetComponent<IPlayer>(out var player))
        {
            tileRender.sortingOrder = 2;
            decoRender.sortingOrder = 3;
            DOVirtual.Color(tile.color, new Color(1, 1, 1, 0.35f), 1,(value) =>
            {
                tile.color = value;
            });
            DOVirtual.Color(deco.color, new Color(1, 1, 1, 0.35f), 1, (value) =>
            {
                deco.color = value;
            });
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<IPlayer>(out var player))
        {
            tileRender.sortingOrder = 0;
            decoRender.sortingOrder = 1;
            DOVirtual.Color(tile.color, new Color(1, 1, 1, 1), 1, (value) =>
            {
                tile.color = value;
            });
            DOVirtual.Color(deco.color, new Color(1, 1, 1, 1), 1, (value) =>
            {
                deco.color = value;
            });
        }
    }
}
