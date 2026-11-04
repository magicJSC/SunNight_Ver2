using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// Fade the visible wall near the player's silhouette. Physics and tile colors are untouched.
public sealed class PlayerWallOcclusion : MonoBehaviour
{
    sealed class Surface
    {
        public Tilemap map;
        public TilemapRenderer renderer;
        public Material original, faded;
        public float amount;
    }

    static readonly int Area = Shader.PropertyToID("_FadeArea");
    static readonly int Amount = Shader.PropertyToID("_FadeAmount");
    const float FadePadding = 1.75f;
    readonly List<Surface> surfaces = new List<Surface>();
    SpriteRenderer body;
    Shader shader;

    void OnEnable()
    {
        body = GetComponent<SpriteRenderer>();
        shader = Resources.Load<Shader>("WallOcclusion");
        foreach (var renderer in Object.FindObjectsByType<TilemapRenderer>(FindObjectsInactive.Include))
        {
            if (renderer.gameObject.scene != gameObject.scene || !IsScenery(renderer.name)) continue;
            var material = renderer.sharedMaterial;
            if (!material || material.shader.name != "Universal Render Pipeline/2D/Sprite-Lit-Default") continue;
            surfaces.Add(new Surface { map = renderer.GetComponent<Tilemap>(), renderer = renderer, original = material });
        }
    }

    static bool IsScenery(string label) =>
        label.Contains("Wall") || label.Contains("Fence") || label == "Roof" || label == "Front" || label == "Back";

    void LateUpdate()
    {
        var bounds = body.bounds;
        foreach (var surface in surfaces)
        {
            if (!surface.renderer) continue;
            bool overlaps = surface.renderer.enabled && surface.map.gameObject.activeInHierarchy && surface.renderer.bounds.Intersects(bounds) &&
                (Occupied(surface.map, bounds.center) ||
                 Occupied(surface.map, bounds.center + Vector3.right * bounds.extents.x * .6f) ||
                 Occupied(surface.map, bounds.center - Vector3.right * bounds.extents.x * .6f) ||
                 Occupied(surface.map, bounds.center - Vector3.up * bounds.extents.y * .8f));
            float target = overlaps ? .77f : 0;
            if (surface.amount == 0 && target == 0) continue;
            surface.amount = Mathf.MoveTowards(surface.amount, target, Time.deltaTime * 4);
            if (surface.amount == 0)
            {
                surface.renderer.sharedMaterial = surface.original;
                continue;
            }
            if (!surface.faded)
                surface.faded = new Material(surface.original) { shader = shader, hideFlags = HideFlags.DontSave };
            if (surface.renderer.sharedMaterial != surface.faded) surface.renderer.sharedMaterial = surface.faded;
            surface.faded.SetVector(Area, new Vector4(bounds.center.x, bounds.center.y,
                bounds.extents.x + FadePadding, bounds.extents.y + FadePadding));
            surface.faded.SetFloat(Amount, surface.amount);
        }
    }

    bool Occupied(Tilemap map, Vector3 point)
    {
        var cell = map.WorldToCell(point);
        return map.HasTile(cell) && transform.position.y >= map.CellToWorld(cell).y;
    }

    void OnDisable()
    {
        foreach (var surface in surfaces)
        {
            if (surface.renderer && surface.renderer.sharedMaterial == surface.faded)
                surface.renderer.sharedMaterial = surface.original;
            if (surface.faded) Destroy(surface.faded);
        }
        surfaces.Clear();
    }
}
