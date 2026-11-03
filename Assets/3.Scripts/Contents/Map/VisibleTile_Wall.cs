using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class VisibleTile_Wall : MonoBehaviour
{
    Tween tileTween, decoTween;

    void OnDisable()
    {
        tileTween?.Kill(false);
        decoTween?.Kill(false);
        tileTween = decoTween = null;
    }

    Tilemap tile;

    public Tilemap deco;
    private void Awake()
    {
        tile = GetComponent<Tilemap>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isActiveAndEnabled || Managers.IsShuttingDown) return;
        if(collision.TryGetComponent<PlayerController>(out var player))
        {
            tileTween?.Kill(false);
            tileTween = DOVirtual.Color(tile.color, new Color(1, 1, 1, 0.3f), 1,(value) =>
            {
                if (tile != null) tile.color = value;
            }).SetRecyclable(false).SetLink(gameObject, LinkBehaviour.KillOnDisable);
            if (deco != null)
            {
                decoTween?.Kill(false);
                decoTween = DOVirtual.Color(deco.color, new Color(1, 1, 1, 0.3f), 1, (value) =>
                {
                    if (deco != null) deco.color = value;
                }).SetRecyclable(false).SetLink(gameObject, LinkBehaviour.KillOnDisable);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!isActiveAndEnabled || Managers.IsShuttingDown) return;
        if (collision.TryGetComponent<PlayerController>(out var player))
        {
            tileTween?.Kill(false);
            tileTween = DOVirtual.Color(tile.color, new Color(1, 1, 1, 1), 1, (value) =>
            {
                if (tile != null) tile.color = value;
            }).SetRecyclable(false).SetLink(gameObject, LinkBehaviour.KillOnDisable);

            if (deco != null)
            {
                decoTween?.Kill(false);
                decoTween = DOVirtual.Color(deco.color, new Color(1, 1, 1, 1), 1, (value) =>
                {
                    if (deco != null) deco.color = value;
                }).SetRecyclable(false).SetLink(gameObject, LinkBehaviour.KillOnDisable);
            }
        }
    }
}
