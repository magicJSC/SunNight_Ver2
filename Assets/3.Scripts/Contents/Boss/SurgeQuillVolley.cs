using System.Collections.Generic;
using UnityEngine;

public sealed class SurgeQuillVolley
{
    const int Count = 5;
    const float Spread = 24f, ChargeTime = 1f, Speed = 7.5f, Range = 12f, Radius = .12f;
    readonly SpriteRenderer boss;
    readonly Sprite charge, release, idle;
    readonly SpriteRenderer[] quills = new SpriteRenderer[Count];
    readonly LineRenderer[] guides = new LineRenderer[Count];
    readonly Vector2[] directions = new Vector2[Count];
    readonly float[] lengths = new float[Count];
    readonly List<RaycastHit2D> hits = new List<RaycastHit2D>();
    readonly ContactFilter2D solids = new ContactFilter2D { useTriggers = false };
    Vector2 origin;
    float elapsed;
    bool dealtDamage;
    public bool Running { get; private set; }

    public SurgeQuillVolley(SpriteRenderer boss, Sprite charge, Sprite release, Sprite quill, Material unlit)
    {
        this.boss = boss;
        this.charge = charge;
        this.release = release;
        idle = boss.sprite;
        for (int i=0; i<Count; i++)
        {
            var shot = new GameObject("Surge quill " + (i+1)).AddComponent<SpriteRenderer>();
            shot.transform.SetParent(boss.transform, false);
            shot.sprite = quill;
            shot.sharedMaterial = unlit;
            shot.sortingOrder = boss.sortingOrder+1;
            shot.enabled = false;
            quills[i] = shot;
            var guide = new GameObject("Quill warning").AddComponent<LineRenderer>();
            guide.transform.SetParent(shot.transform, false);
            guide.useWorldSpace = true;
            guide.positionCount = 2;
            guide.sharedMaterial = unlit;
            guide.startWidth = .07f;
            guide.endWidth = .04f;
            guide.startColor = new Color(1,.79f,.37f,.9f);
            guide.endColor = new Color(1,.73f,.33f,.48f);
            guide.sortingOrder = boss.sortingOrder-1;
            guide.enabled = false;
            guides[i] = guide;
        }
    }

    public void Begin(Vector2 target)
    {
        boss.flipX = target.x > boss.transform.position.x;
        origin = (Vector2)boss.transform.position + new Vector2(boss.flipX ? -.34f : .34f, 1.95f);
        float aim = Mathf.Atan2(target.y-origin.y,target.x-origin.x)*Mathf.Rad2Deg;
        for (int i=0; i<Count; i++)
        {
            var rotation = Quaternion.Euler(0,0,aim+(i-Count/2)*Spread);
            Vector2 direction = rotation*Vector2.right;
            directions[i] = direction;
            float length = Range;
            Physics2D.CircleCast(origin, Radius, direction, solids, hits, Range);
            foreach (var hit in hits)
                if (hit.collider.transform.IsChildOf(boss.transform.parent))
                    length = Mathf.Min(length,hit.distance);
            lengths[i] = length;
            guides[i].SetPosition(0,origin);
            guides[i].SetPosition(1,origin+direction*length);
            guides[i].enabled = length > 0;
            quills[i].transform.SetPositionAndRotation(origin,rotation);
        }
        boss.sprite = charge;
        elapsed = 0;
        dealtDamage = false;
        Running = true;
    }

    public void Tick(PlayerController player, Collider2D feet)
    {
        float previous = Mathf.Max(0,(elapsed-ChargeTime)*Speed);
        elapsed += Time.deltaTime;
        if (elapsed < ChargeTime) return;
        float distance = (elapsed-ChargeTime)*Speed;
        boss.sprite = elapsed < ChargeTime+.24f ? release : idle;
        for (int i=0; i<Count; i++)
        {
            guides[i].enabled = false;
            if (previous >= lengths[i]) continue;
            float next = Mathf.Min(distance,lengths[i]);
            Vector2 from = origin+directions[i]*previous;
            if (!dealtDamage)
            {
                Physics2D.CircleCast(from,Radius,directions[i],solids,hits,next-previous);
                foreach (var hit in hits)
                {
                    if (hit.collider != feet || hit.distance+previous >= lengths[i]) continue;
                    dealtDamage = true;
                    lengths[i] = next;
                    player.GetDamage(14);
                    break;
                }
            }
            quills[i].transform.position = origin+directions[i]*next;
            quills[i].enabled = next < lengths[i];
        }
        if (distance >= Range) Stop();
    }

    public void Stop()
    {
        for (int i=0; i<Count; i++) guides[i].enabled = quills[i].enabled = false;
        boss.sprite = idle;
        Running = false;
    }
}
