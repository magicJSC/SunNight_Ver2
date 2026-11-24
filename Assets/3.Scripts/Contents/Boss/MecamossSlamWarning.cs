using System.Collections.Generic;
using UnityEngine;

// One clipped polygon supplies both the visible floor boundary and hit shape.
public sealed class MecamossSlamWarning : MonoBehaviour
{
    const int Segments = 32;
    Mesh mesh;
    Material material;
    MeshRenderer fill;
    LineRenderer outline, wave;
    PolygonCollider2D area;
    BoxCollider2D[] walls;
    BoxCollider2D barge;
    Vector2[] points;
    readonly Color[] fillColors = new Color[Segments + 2];

    public static MecamossSlamWarning Create(Transform hall, BoxCollider2D[] walls, BoxCollider2D barge)
    {
        var go = new GameObject(nameof(MecamossSlamWarning));
        go.transform.SetParent(hall, false);
        go.layer = 2;
        var marker = go.AddComponent<MecamossSlamWarning>();
        marker.walls = walls;
        marker.barge = barge;
        marker.mesh = new Mesh { name = "Mecamoss slam sector" };
        go.AddComponent<MeshFilter>().sharedMesh = marker.mesh;
        marker.material = new Material(Shader.Find("Sprites/Default"));
        marker.fill = go.AddComponent<MeshRenderer>();
        marker.fill.sharedMaterial = marker.material;
        marker.fill.sortingOrder = -1;
        marker.area = go.AddComponent<PolygonCollider2D>();
        marker.area.isTrigger = true;
        marker.outline = marker.Line("Boundary", .055f, true);
        marker.wave = marker.Line("Impact front", .12f, false);
        marker.Hide();
        return marker;
    }

    LineRenderer Line(string name, float width, bool loop)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);
        var line = go.AddComponent<LineRenderer>();
        line.sharedMaterial = material;
        line.useWorldSpace = false;
        line.loop = loop;
        line.widthMultiplier = width;
        line.sortingOrder = 0;
        return line;
    }

    public void Show(Vector2 origin, Vector2 direction)
    {
        transform.position = origin;
        transform.rotation = Quaternion.identity;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        points = new Vector2[Segments + 2];
        var vertices = new Vector3[points.Length];
        var triangles = new List<int>(Segments * 3);
        for (int i = 0; i <= Segments; i++)
        {
            float a = (angle - 40f + 80f * i / Segments) * Mathf.Deg2Rad;
            Vector2 ray = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
            float distance = 6;
            foreach (var hit in Physics2D.RaycastAll(origin, ray, 6))
                if (hit.collider == barge || System.Array.IndexOf(walls, hit.collider) >= 0)
                    distance = Mathf.Min(distance, Mathf.Max(.02f, hit.distance));
            points[i + 1] = ray * distance;
            vertices[i + 1] = points[i + 1];
            if (i > 0) { triangles.Add(0); triangles.Add(i); triangles.Add(i + 1); }
        }
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateBounds();
        area.SetPath(0, points);
        area.enabled = true;
        outline.positionCount = vertices.Length;
        outline.SetPositions(vertices);
        fill.enabled = outline.enabled = true;
        wave.enabled = false;
        Telegraph(0);
    }

    void SetFill(Color color)
    {
        for (int i = 0; i < fillColors.Length; i++) fillColors[i] = color;
        mesh.colors = fillColors;
    }

    public void Telegraph(float progress)
    {
        SetFill(new Color(1f, .6f, .16f, .10f + .12f * Mathf.Clamp01(progress)));
        outline.startColor = outline.endColor = new Color(1f, .8f, .35f, 1f);
    }

    public void Impact(float radius)
    {
        SetFill(new Color(1f, .78f, .35f, .45f));
        wave.enabled = true;
        wave.startColor = wave.endColor = Color.white;
        wave.positionCount = Segments + 1;
        for (int i = 1; i < points.Length; i++)
            wave.SetPosition(i - 1, points[i].normalized * Mathf.Min(radius, points[i].magnitude));
    }

    public bool Overlaps(BoxCollider2D feet, float radius)
    {
        return area.enabled && area.Distance(feet).isOverlapped &&
            ((Vector2)feet.ClosestPoint(transform.position) - (Vector2)transform.position).sqrMagnitude <= radius * radius;
    }

    public void Hide()
    {
        if (fill) fill.enabled = false;
        if (outline) outline.enabled = false;
        if (wave) wave.enabled = false;
        if (area) area.enabled = false;
    }

    void OnDestroy()
    {
        if (mesh) Destroy(mesh);
        if (material) Destroy(material);
    }
}
