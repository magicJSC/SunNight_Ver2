using System;
using UnityEngine;

// A ground marker. The combat controller owns the target, radius and clock.
[RequireComponent(typeof(SpriteRenderer))]
public sealed class MecamossBreachWarning : MonoBehaviour
{
    SpriteRenderer ink;
    Sprite[] ripples;

    public static MecamossBreachWarning Create(Transform hall)
    {
        var marker = new GameObject(nameof(MecamossBreachWarning));
        marker.transform.SetParent(hall, false);
        var warning = marker.AddComponent<MecamossBreachWarning>();
        warning.ink = marker.GetComponent<SpriteRenderer>();
        // Receiving hall floor = -18, player group = 1, foreman = 2.
        warning.ink.sortingOrder = 0;
        warning.ripples = Resources.LoadAll<Sprite>("Marsh/BreachRipple");
        Array.Sort(warning.ripples, (a, b) => string.CompareOrdinal(a.name, b.name));
        warning.Hide();
        return warning;
    }

    public void Show(Vector2 worldCenter, float worldRadius)
    {
        transform.position = worldCenter;
        var frame = ripples[0];
        float diameter = frame.rect.width / frame.pixelsPerUnit;
        float scale = worldRadius * 2f / diameter;
        Vector3 parentScale = transform.parent.lossyScale;
        transform.localScale = new Vector3(scale / parentScale.x, scale / parentScale.y, 1f);
        ink.sprite = frame;
        ink.enabled = true;
    }

    public void SetProgress(float normalized)
    {
        int frame = Mathf.Min(Mathf.FloorToInt(Mathf.Clamp01(normalized) * ripples.Length), ripples.Length - 1);
        ink.sprite = ripples[frame];
    }

    public void Hide() => ink.enabled = false;
}
