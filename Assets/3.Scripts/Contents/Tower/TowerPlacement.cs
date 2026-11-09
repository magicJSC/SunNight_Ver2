using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// The preview and both placement paths use the occupied cells of the same packed tower.
internal sealed class TowerPlacement
{
    static readonly Vector2Int[] Directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
    readonly HashSet<Vector2Int> footprint = new();
    readonly Tilemap ground;
    readonly Tilemap forbidden;
    readonly Transform player;
    readonly Vector2 playerSize;
    readonly List<Collider2D> overlaps = new();
    readonly List<RaycastHit2D> obstructions = new();
    readonly ContactFilter2D solids = new ContactFilter2D { useTriggers = false };
    readonly Queue<Vector2Int> frontier = new();
    readonly HashSet<Vector2Int> visited = new();
    readonly List<Vector2Int> exits = new();
    Vector2Int searchOrigin;
    bool searching;

    internal TowerPlacement(Tilemap preview)
    {
        player = Managers.Game.player.transform;
        playerSize = player.GetComponent<Collider2D>().bounds.size;
        forbidden = MapManager.cantBuild;
        var grid = Managers.Game.grid.transform;
        var terrain = grid.Find("Tilemap") ?? grid.Find("Ground");
        if (terrain) ground = terrain.GetComponent<Tilemap>();
        var tile = MapManager.tower.GetTile(new Vector3Int(-1, -1, 0));
        foreach (var map in new[] { MapManager.tower, MapManager.building })
            foreach (var cell in map.cellBounds.allPositionsWithin)
                if (map.HasTile(cell))
                {
                    footprint.Add((Vector2Int)cell);
                    preview.SetTile(cell, tile);
                }

        // Candidate standing positions on the outside edge, including clearance for the player collider.
        var unique = new HashSet<Vector2Int>();
        foreach (var cell in footprint)
            foreach (var direction in Directions)
            {
                if (footprint.Contains(cell + direction)) continue;
                unique.Add(cell + direction);
                unique.Add(cell + direction * 2);
            }
        exits.AddRange(unique);
    }

    bool HasGround(Vector2 point) => !ground || ground.HasTile(ground.WorldToCell(point));

    bool Clear(Vector2 point, Vector2 size)
    {
        Physics2D.OverlapBox(point, size, 0, solids, overlaps);
        foreach (var hit in overlaps)
            if (!hit.transform.IsChildOf(player)) return false;
        return true;
    }

    bool ReachableInLine(Vector2 from, Vector2 to)
    {
        Vector2 travel = to - from;
        if (travel.sqrMagnitude < .0001f) return true;
        Physics2D.BoxCast(from, playerSize * .94f, 0, travel.normalized, solids, obstructions, travel.magnitude);
        foreach (var hit in obstructions)
            if (!hit.transform.IsChildOf(player)) return false;
        return true;
    }

    bool FitsPlayer(Vector2 position, Vector2Int anchor)
    {
        Vector2 half = playerSize * .5f;
        Vector2Int min = Vector2Int.FloorToInt(position - half - (Vector2)anchor);
        Vector2Int max = Vector2Int.FloorToInt(position + half - (Vector2)anchor);
        for (int y = min.y; y <= max.y; y++)
            for (int x = min.x; x <= max.x; x++)
                if (footprint.Contains(new Vector2Int(x, y))) return false;
        return true;
    }

    internal bool CanPlace(Vector2Int anchor, out Vector2 playerPosition)
    {
        playerPosition = player.position;
        foreach (var offset in footprint)
        {
            Vector2 center = (Vector2)(anchor + offset) + Vector2.one * .5f;
            if (!HasGround(center) || forbidden.HasTile(forbidden.WorldToCell(center)) ||
                !Clear(center, Vector2.one * .97f)) return false;
        }
        // Placing at the player's feet is valid if they can step clear of the new footprint.
        bool needsExit = !FitsPlayer(playerPosition, anchor);
        float closest = float.PositiveInfinity;
        foreach (var offset in exits)
        {
            Vector2 point = (Vector2)(anchor + offset) + Vector2.one * .5f;
            float distance = (point - (Vector2)player.position).sqrMagnitude;
            if (distance >= closest || !FitsPlayer(point, anchor) || !HasGround(point) || !Clear(point, playerSize)) continue;
            if (!needsExit) return true;
            if (!ReachableInLine(player.position, point)) continue;
            closest = distance;
            playerPosition = point;
        }
        return !float.IsPositiveInfinity(closest);
    }

    internal bool Manual(Vector2Int anchor, out Vector2 playerPosition)
    {
        playerPosition = player.position;
        return (playerPosition - (Vector2)anchor).sqrMagnitude <= 4f &&
            ReachableInLine(playerPosition, anchor) && CanPlace(anchor, out playerPosition);
    }

    internal bool Nearest(out Vector2Int anchor, out Vector2 playerPosition)
    {
        var origin = Vector2Int.RoundToInt(player.position);
        anchor = origin;
        playerPosition = player.position;
        if (!HasGround(player.position)) { searching = false; return false; }
        if (!searching || origin != searchOrigin)
        {
            frontier.Clear();
            visited.Clear();
            frontier.Enqueue(origin);
            visited.Add(origin);
            searchOrigin = origin;
            searching = true;
        }

        // Visit reachable anchors by walking distance. Resume big searches across clock retries.
        for (int i = 0; i < 256 && frontier.Count > 0; i++)
        {
            anchor = frontier.Dequeue();
            if (CanPlace(anchor, out playerPosition)) { searching = false; return true; }
            Vector2 from = anchor == origin ? (Vector2)player.position : (Vector2)anchor;
            foreach (var direction in Directions)
            {
                var next = anchor + direction;
                if (visited.Contains(next) || !HasGround(next) || !Clear(next, playerSize) || !ReachableInLine(from, next)) continue;
                visited.Add(next);
                frontier.Enqueue(next);
            }
        }
        searching = frontier.Count != 0;
        return false;
    }
}
