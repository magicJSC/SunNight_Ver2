using UnityEngine;

public sealed class MarshDockBearing : MonoBehaviour
{
    [SerializeField] Transform unloadingCrane;
    [SerializeField] Material bearingMaterial;
    SpriteRenderer[] landmarks;
    SpriteRenderer bearing;
    Camera fieldCamera;
    float lastSighting;

    void Start()
    {
        landmarks = GetComponentsInChildren<SpriteRenderer>();
        fieldCamera = Camera.main;
        var marker = new GameObject("MarshDockBearing");
        marker.transform.SetParent(transform, false);
        bearing = marker.AddComponent<SpriteRenderer>();
        bearing.sharedMaterial = bearingMaterial;
        bearing.sprite = Resources.Load<Sprite>("Marsh/CraneBearing");
        bearing.sortingLayerName = "UI";
        bearing.color = new Color(1, 1, 1, 0);
        bearing.enabled = false;
    }

    void LateUpdate()
    {
        var state = FieldSession.State;
        if (!FieldSession.Active || !state.foxtonReturnSlipCollected || state.northwestFactoryGateDiscovered ||
            !Managers.HasInstance || Managers.Game.player == null || !fieldCamera || !unloadingCrane ||
            FieldHUD.IsOpen || Managers.Game.isCantPlay || PlayerController.isDie ||
            Managers.UI.PopUIList.Exists(window => window && window.activeInHierarchy))
        {
            bearing.enabled = false;
            bearing.color = new Color(1, 1, 1, 0);
            return;
        }

        bool searchingMarsh = NorthwestMarsh.HasGround(Managers.Game.player.transform.position);
        if (!searchingMarsh && !bearing.enabled) return;
        if (searchingMarsh && SeesLandmark()) lastSighting = Time.unscaledTime;
        bool show = searchingMarsh && Time.unscaledTime - lastSighting > .4f;
        float alpha = Mathf.MoveTowards(bearing.color.a, show ? 1 : 0, Time.unscaledDeltaTime * 5);
        bearing.enabled = alpha > 0;
        if (!bearing.enabled) { bearing.color = new Color(1, 1, 1, 0); return; }

        Vector3 viewport = fieldCamera.WorldToViewportPoint(unloadingCrane.position);
        Vector2 direction = (Vector2)viewport - Vector2.one * .5f;
        // Inset the frame above the hotbar and below the mission banner.
        float edge = Mathf.Max(Mathf.Abs(direction.x) / .435f,
            Mathf.Abs(direction.y) / (direction.y > 0 ? .36f : .285f));
        if (edge < .001f) { bearing.enabled = false; return; }
        Vector2 pin = Vector2.one * .5f + direction / edge;
        bearing.transform.position = fieldCamera.ViewportToWorldPoint(new Vector3(pin.x, pin.y, viewport.z));
        bearing.transform.localScale = Vector3.one * (fieldCamera.orthographicSize * .061f);
        bearing.color = new Color(1, 1, 1, alpha);
    }

    bool SeesLandmark()
    {
        // Require a readable portion inside the Game camera. Scene view visibility is unrelated.
        foreach (var landmark in landmarks)
        {
            Vector3 p = fieldCamera.WorldToViewportPoint(landmark.bounds.center);
            if (p.z > 0 && p.x > .035f && p.x < .965f && p.y > .07f && p.y < .93f) return true;
        }
        return false;
    }
}
