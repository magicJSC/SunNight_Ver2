using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class SouthwestSwitchyardCheck
{
    [MenuItem("SunNight/Level Design/Check Southwest Routes")]
    public static void Check()
    {
        var yard = Object.FindAnyObjectByType<SouthwestSwitchyard>();
        var oldGround = GameObject.Find("Grid/Tilemap").GetComponent<Tilemap>();
        var ground = yard.GetComponent<Tilemap>();
        var feeders = yard.transform.parent.GetComponentsInChildren<SurgeLiveCable>();
        Physics2D.SyncTransforms();
        var marsh = Object.FindAnyObjectByType<NorthwestMarsh>().GetComponent<Tilemap>();
        var area = new RectInt(-358, -311, 450, 225);
        foreach (var cell in area.allPositionsWithin)
            if (!ground.HasTile((Vector3Int)cell) && !oldGround.HasTile((Vector3Int)cell) && !marsh.HasTile((Vector3Int)cell))
                throw new System.InvalidOperationException($"Uncovered southwest cell {cell}");
        var blocked = new HashSet<Vector2Int>();
        foreach (var collider in yard.transform.parent.GetComponentsInChildren<BoxCollider2D>())
        {
            var bounds = collider.bounds;
            var lo = Vector2Int.FloorToInt((Vector2)bounds.min - Vector2.one);
            var hi = Vector2Int.CeilToInt((Vector2)bounds.max + Vector2.one);
            for (int y = lo.y; y <= hi.y; y++)
                for (int x = lo.x; x <= hi.x; x++)
                    if (bounds.SqrDistance(new Vector3(x + .5f, y + .5f)) < .36f)
                        blocked.Add(new Vector2Int(x, y));
        }
        var gate = Vector2Int.FloorToInt(yard.entrance.position);
        var reachable = new HashSet<Vector2Int> { gate };
        var frontier = new Queue<Vector2Int>();
        frontier.Enqueue(gate);
        Vector2Int[] directions = {Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right};
        while (frontier.Count > 0)
        {
            var cell = frontier.Dequeue();
            foreach (var direction in directions)
            {
                var next = cell + direction;
                if (reachable.Contains(next) || !area.Contains(next)) continue;
                var point = (Vector2)next + Vector2.one * .5f;
                if (!ground.HasTile(ground.WorldToCell(point)) && !oldGround.HasTile(oldGround.WorldToCell(point))) continue;
                // Include live bands as obstacles so both routes work even during a discharge.
                if (blocked.Contains(next)) continue;
                reachable.Add(next);
                frontier.Enqueue(next);
            }
        }
        var north = new Vector2Int(-300, -155);
        var east = new Vector2Int(-137, -217);
        if (!oldGround.HasTile((Vector3Int)north) || !oldGround.HasTile((Vector3Int)east) ||
            !reachable.Contains(north) || !reachable.Contains(east) || feeders.Length != 9 ||
            !reachable.Contains(new Vector2Int(-353,-304)) || !reachable.Contains(new Vector2Int(78,-306)) ||
            yard.rewards.Any(r => !reachable.Contains(Vector2Int.FloorToInt(r.position))))
            throw new System.InvalidOperationException("Check the southwest approaches, safe routes and supply sites.");
        Debug.Log($"SOUTHWEST_ROUTES_OK: {reachable.Count} connected cells, both approaches and all supply sites.");
    }
}

