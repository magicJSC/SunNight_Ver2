using UnityEngine;

public sealed class SurgeGroundStrike
{
    const float Tell = 1.15f, Flash = .32f, Radius = 2f;
    readonly SpriteRenderer boss, warning, bolt;
    readonly Sprite cast, idle;
    float elapsed;
    public bool Running => warning.enabled || bolt.enabled;

    public SurgeGroundStrike(SpriteRenderer boss, Sprite cast, Sprite marker, Sprite impact, Material unlit)
    {
        this.boss = boss;
        this.cast = cast;
        idle = boss.sprite;
        SpriteRenderer Effect(string name, Sprite sprite, int order)
        {
            var effect = new GameObject(name).AddComponent<SpriteRenderer>();
            effect.transform.SetParent(boss.transform, false);
            effect.sprite = sprite;
            effect.sharedMaterial = unlit;
            effect.sortingLayerID = boss.sortingLayerID;
            effect.sortingOrder = order;
            effect.enabled = false;
            return effect;
        }
        warning = Effect("Ground strike warning", marker, -1);
        bolt = Effect("Ground strike bolt", impact, boss.sortingOrder + 1);
    }

    public void Begin(Vector2 target)
    {
        elapsed = 0;
        boss.flipX = target.x > boss.transform.position.x;
        boss.sprite = cast;
        warning.transform.position = bolt.transform.position = target;
        warning.color = new Color(1,.8f,.55f,.72f);
        warning.enabled = true;
    }

    public void Tick(PlayerController player, Collider2D feet)
    {
        elapsed += Time.deltaTime;
        if (elapsed < Tell)
        {
            warning.color = Color.Lerp(new Color(1,.8f,.55f,.72f), Color.white, elapsed/Tell);
            return;
        }
        if (warning.enabled)
        {
            warning.enabled = false;
            bolt.enabled = true;
            Vector2 center = bolt.transform.position;
            if ((feet.ClosestPoint(center)-center).sqrMagnitude <= Radius*Radius)
                player.GetDamage(20);
        }
        if (elapsed >= Tell + Flash) Stop();
    }

    public void Stop()
    {
        bolt.enabled = warning.enabled = false;
        boss.sprite = idle;
    }
}
