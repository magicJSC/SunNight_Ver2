using UnityEngine;

// Convert through screen coordinates so nested canvases and panel pivots use the same bounds.
public static class UIScreenPlacement
{
    static Camera CameraFor(RectTransform rect)
    {
        var canvas = rect.GetComponentInParent<Canvas>().rootCanvas;
        return canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
    }

    public static Rect Bounds(RectTransform rect)
    {
        var corners = new Vector3[4];
        rect.GetWorldCorners(corners);
        var camera = CameraFor(rect);
        Vector2 min = RectTransformUtility.WorldToScreenPoint(camera, corners[0]);
        Vector2 max = RectTransformUtility.WorldToScreenPoint(camera, corners[2]);
        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }

    static void Shift(RectTransform rect, Vector2 delta)
    {
        var parent = (RectTransform)rect.parent;
        Vector2 screen = RectTransformUtility.WorldToScreenPoint(CameraFor(rect), rect.position);
        RectTransformUtility.ScreenPointToWorldPointInRectangle(parent, screen + delta, CameraFor(rect), out var world);
        rect.position = world;
    }

    public static void Clamp(RectTransform rect)
    {
        var bounds = Bounds(rect);
        var safe = Screen.safeArea;
        float x = Mathf.Clamp(bounds.xMin, safe.xMin + 8, Mathf.Max(safe.xMin + 8, safe.xMax - bounds.width - 8));
        float y = Mathf.Clamp(bounds.yMin, safe.yMin + 8, Mathf.Max(safe.yMin + 8, safe.yMax - bounds.height - 8));
        Shift(rect, new Vector2(x - bounds.xMin, y - bounds.yMin));
    }

    public static void OpenInventory(RectTransform panel)
    {
        Canvas.ForceUpdateCanvases();
        var bounds = Bounds(panel);
        float scale = panel.GetComponentInParent<Canvas>().rootCanvas.scaleFactor;
        Shift(panel, new Vector2(Screen.safeArea.xMax - 20 * scale - bounds.xMax,
            Screen.safeArea.yMax - 35 * scale - bounds.yMax));
        Clamp(panel);
    }

    public static void OpenCrafting(RectTransform panel, RectTransform inventory)
    {
        Canvas.ForceUpdateCanvases();
        OpenInventory(inventory);
        var right = Bounds(inventory);
        var left = Bounds(panel);
        float scale = panel.GetComponentInParent<Canvas>().rootCanvas.scaleFactor;
        Shift(panel, new Vector2(right.xMin - 10 * scale - left.xMax, right.yMax - 5 * scale - left.yMax));
        Clamp(panel);
    }

    public static void Tooltip(RectTransform tooltip, RectTransform source)
    {
        var parentCanvas = tooltip.parent.GetComponentInParent<Canvas>();
        var canvas = tooltip.GetComponent<Canvas>();
        if (canvas == null) canvas = tooltip.gameObject.AddComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingLayerID = parentCanvas.sortingLayerID;
        canvas.sortingOrder = 100;
        var group = tooltip.GetComponent<CanvasGroup>();
        if (group == null) group = tooltip.gameObject.AddComponent<CanvasGroup>();
        group.blocksRaycasts = false;
        group.interactable = false;
        Canvas.ForceUpdateCanvases();
        var item = Bounds(source);
        var tip = Bounds(tooltip);
        float x = item.xMin - tip.width - 12;
        if (x < Screen.safeArea.xMin + 8) x = item.xMax + 12;
        Shift(tooltip, new Vector2(x - tip.xMin, item.yMax - tip.yMax));
        Clamp(tooltip);
    }
}
