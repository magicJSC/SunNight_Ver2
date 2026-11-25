using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public sealed class SurgeLiveCable : MonoBehaviour
{
    public SpriteRenderer warning, arc;
    public float offset;
    BoxCollider2D band;
    int lastHitCycle = -1;
    float elapsed;

    void Awake() => band = GetComponent<BoxCollider2D>();

    System.Collections.IEnumerator Start()
    {
        // Reserve all touched ground cells for both manual and dusk deployment.
        while (!MapManager.cantBuild || !Managers.Map.canBuildTile) yield return null;
        var map = MapManager.cantBuild;
        var bounds = band.bounds;
        var lo = map.WorldToCell(bounds.min + Vector3.one * .001f);
        var hi = map.WorldToCell(bounds.max - Vector3.one * .001f);
        for (int y = lo.y; y <= hi.y; y++)
            for (int x = lo.x; x <= hi.x; x++)
                map.SetTile(new Vector3Int(x, y, 0), Managers.Map.canBuildTile);
    }

    void Update()
    {
        if (!Managers.HasInstance || !Managers.Game.player || Managers.Game.isCantPlay || FieldHUD.IsOpen) return;
        elapsed += Time.deltaTime;
        float clock = elapsed + offset;
        int cycle = Mathf.FloorToInt(clock / 6.5f);
        float phase = clock - cycle * 6.5f;
        warning.enabled = phase < 1.5f;
        warning.color = phase % .75f < .38f ? Color.white : new Color(.75f, .7f, .6f, .6f);
        arc.enabled = phase >= 1.5f && phase < 2.5f;
        var player = Managers.Game.player;
        if (arc.enabled && lastHitCycle != cycle && !PlayerController.isDie &&
            band.Distance(player.GetComponent<Collider2D>()).isOverlapped)
        {
            lastHitCycle = cycle;
            player.GetDamage(8);
        }
    }
}

