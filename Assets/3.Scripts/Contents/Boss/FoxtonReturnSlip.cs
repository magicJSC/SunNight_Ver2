using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class FoxtonReturnSlip
{
    // Center of the playable ring in Boss1, also the initial defensive base position.
    static readonly Vector2Int ArenaCenter = new Vector2Int(1, -2);
    static readonly Vector2Int[] Directions = { Vector2Int.up, Vector2Int.right, Vector2Int.left, Vector2Int.down };

    public static void Drop(GameObject returnTimeline)
    {
        var slip = new GameObject("FoxtonReturnSlip");
        slip.transform.position = FindReachableCenter();
        var paper = slip.AddComponent<SpriteRenderer>();
        paper.sprite = Resources.Load<Sprite>("Story/FoxtonReturnSlip");
        paper.sortingLayerName = "Deco";
        paper.sortingOrder = 5;
        var trigger = slip.AddComponent<CircleCollider2D>();
        trigger.isTrigger = true;
        trigger.radius = .65f;

        slip.AddComponent<LabInteraction>().Configure("반송 전표 · F 줍고 지상으로", () =>
        {
            if (!slip || !slip.activeSelf || FieldSession.State.foxtonReturnSlipCollected) return;
            FieldSession.State.foxtonReturnSlipCollected = true;
            slip.SetActive(false);
            Object.Destroy(slip);
            Object.Instantiate(returnTimeline);
        });
        FieldHUD.Notify("투기장 중앙에 반송 전표가 떨어졌다.");
    }

    // Start from the player's side of any walls they built. A free tile enclosed by
    // facilities is still unreachable, even if it is the closest tile to the center.
    static Vector2 FindReachableCenter()
    {
        var player = Managers.Game.player;
        var floor = Managers.Game.grid.transform.Find("Ground").GetComponent<Tilemap>();
        Vector2 start = player.transform.position;
        Vector2 bodySize = player.GetComponent<Collider2D>().bounds.size;
        var solid = new ContactFilter2D { useTriggers = false };
        var hits = new List<RaycastHit2D>();
        var open = new Queue<Vector2Int>();
        var visited = new HashSet<Vector2Int>();
        Vector2Int origin = Vector2Int.RoundToInt(start);
        open.Enqueue(origin);
        visited.Add(origin);
        Vector2 nearest = start;
        float closest = (start - (Vector2)ArenaCenter).sqrMagnitude;
        while (open.Count > 0)
        {
            var cell = open.Dequeue();
            Vector2 from = cell == origin ? start : (Vector2)cell;
            foreach (var step in Directions)
            {
                var next = cell + step;
                if (visited.Contains(next) || !floor.HasTile(floor.WorldToCell((Vector2)next))) continue;
                Vector2 travel = (Vector2)next - from;
                Physics2D.BoxCast(from, bodySize, 0f, travel.normalized, solid, hits, travel.magnitude);
                if (hits.Exists(hit => !hit.transform.IsChildOf(player.transform))) continue;
                visited.Add(next);
                int distance = (next - ArenaCenter).sqrMagnitude;
                if (distance == 0) return next;
                if (distance < closest)
                {
                    closest = distance;
                    nearest = next;
                }
                open.Enqueue(next);
            }
        }
        return nearest;
    }
}
