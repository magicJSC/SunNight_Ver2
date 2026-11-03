using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class InsideBuildTrigger : MonoBehaviour
{
    Tween roofTween, wallTween;

    void OnDisable()
    {
        roofTween?.Kill(false);
        wallTween?.Kill(false);
        roofTween = wallTween = null;
    }

    public Tilemap roof;
    public Tilemap wall;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isActiveAndEnabled || Managers.IsShuttingDown) return;
        if (collision.TryGetComponent<PlayerController>(out var player))
        {
            if (roof != null)
            {
                roofTween?.Kill(false);
                roofTween = DOVirtual.Color(roof.color, new Color(1, 1, 1, 0), 1, (value) =>
                {
                    if (roof != null) roof.color = value;
                }).SetRecyclable(false).SetLink(gameObject, LinkBehaviour.KillOnDisable);
            }

            if(wall != null)
            {
                wallTween?.Kill(false);
                wallTween = DOVirtual.Color(wall.color, new Color(1, 1, 1, 0.3f), 1, (value) =>
                {
                    if (wall != null) wall.color = value;
                }).SetRecyclable(false).SetLink(gameObject, LinkBehaviour.KillOnDisable);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!isActiveAndEnabled || Managers.IsShuttingDown) return;
        if (collision.TryGetComponent<PlayerController>(out var player))
        {
            if (roof != null)
            {
                roofTween?.Kill(false);
                roofTween = DOVirtual.Color(roof.color, new Color(1, 1, 1, 1), 1, (value) =>
                {
                    if (roof != null) roof.color = value;
                }).SetRecyclable(false).SetLink(gameObject, LinkBehaviour.KillOnDisable);
            }

            if (wall != null)
            {
                wallTween?.Kill(false);
                wallTween = DOVirtual.Color(wall.color, new Color(1, 1, 1, 1), 1, (value) =>
                {
                    if (wall != null) wall.color = value;
                }).SetRecyclable(false).SetLink(gameObject, LinkBehaviour.KillOnDisable);
            }
        }
    }
}
