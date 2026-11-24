using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public sealed class NorthwestMarsh : MonoBehaviour
{
    [SerializeField] TileBase shallows;
    static NorthwestMarsh active;
    Tilemap terrain;

    void OnEnable()
    {
        terrain = GetComponent<Tilemap>();
        active = this;
    }

    void OnDisable()
    {
        if (active == this) active = null;
    }

    public static bool HasGround(Vector2 point)
        => active && active.terrain.HasTile(active.terrain.WorldToCell(point));

    public static float WalkSpeedAt(Vector2 point)
    {
        if (!active || !active.shallows) return 1f;
        var hall = MecamossReceivingHall.Current;
        var map = hall && hall.Contains(point) ? hall.Floor : active.terrain;
        return map.GetTile(map.WorldToCell(point)) == active.shallows ? .85f : 1f;
    }
}
