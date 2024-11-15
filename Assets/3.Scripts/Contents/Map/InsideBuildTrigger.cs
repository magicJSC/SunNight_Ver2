using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class InsideBuildTrigger : MonoBehaviour
{
    public Tilemap roof;
    public Tilemap wall;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<PlayerController>(out var player))
        {
            if (roof != null)
            {
                DOVirtual.Color(roof.color, new Color(1, 1, 1, 0), 1, (value) =>
                {
                    roof.color = value;
                });
            }

            if(wall != null)
            {
                DOVirtual.Color(wall.color, new Color(1, 1, 1, 0.3f), 1, (value) =>
                {
                    wall.color = value;
                });
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<PlayerController>(out var player))
        {
            if (roof != null)
            {
                DOVirtual.Color(roof.color, new Color(1, 1, 1, 1), 1, (value) =>
                {
                    roof.color = value;
                });
            }

            if (wall != null)
            {
                DOVirtual.Color(wall.color, new Color(1, 1, 1, 1), 1, (value) =>
                {
                    wall.color = value;
                });
            }
        }
    }
}
