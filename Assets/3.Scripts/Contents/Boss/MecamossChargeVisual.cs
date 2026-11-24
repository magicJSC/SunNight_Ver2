using System.Collections.Generic;
using UnityEngine;

// The encounter supplies the locked line and its travel time. The visual never
// moves or turns the boss transform, which is also the ground and hitbox root.
public sealed class MecamossChargeVisual
{
    static readonly int Scrape = Animator.StringToHash("Base Layer.ChargeScrape");
    static readonly int Gallop = Animator.StringToHash("Base Layer.ChargeRush");
    static readonly int Brake = Animator.StringToHash("Base Layer.ChargeBrake");
    static readonly int Crash = Animator.StringToHash("Base Layer.ChargeCrash");
    const float MarkerSpacing = 1.5f;

    readonly SpriteRenderer boss;
    readonly Animator animator;
    readonly Sprite laneTile;
    readonly Transform lane;
    readonly List<SpriteRenderer> segments = new List<SpriteRenderer>();
    Sprite previousPose;
    AnimatorStateInfo previousState;
    bool previousFlip, active;

    public MecamossChargeVisual(SpriteRenderer boss, Animator animator)
    {
        this.boss = boss;
        this.animator = animator;
        laneTile = Resources.Load<Sprite>("Marsh/MecamossChargeLane");
        lane = new GameObject("MecamossChargeLane").transform;
        lane.SetParent(boss.transform.parent, false);
        lane.gameObject.SetActive(false);
    }

    public void Begin(Vector2 origin, Vector2 direction, float distance, float radius)
    {
        if (!active)
        {
            previousPose = boss.sprite;
            previousFlip = boss.flipX;
            previousState = animator.GetCurrentAnimatorStateInfo(0);
        }
        active = true;
        if (Mathf.Abs(direction.x) > .01f) boss.flipX = direction.x > 0;
        Play(Scrape);
        ShowLane(origin, direction, distance, radius);
    }

    void Play(int state)
    {
        animator.Play(state, 0, 0);
        animator.Update(0);
    }

    public void Rush()
    {
        lane.gameObject.SetActive(false);
        Play(Gallop);
    }

    public void Recover(bool crashed)
    {
        Play(crashed ? Crash : Brake);
    }

    public void Stop()
    {
        if (lane) lane.gameObject.SetActive(false);
        if (!active) return;
        active = false;
        if (!boss || !animator) return;
        boss.flipX = previousFlip;
        if (animator.isActiveAndEnabled)
        {
            animator.Play(previousState.fullPathHash, 0, previousState.normalizedTime);
            animator.Update(0);
        }
        else boss.sprite = previousPose;
    }

    void ShowLane(Vector2 origin, Vector2 direction, float length, float radius)
    {
        int count = Mathf.CeilToInt(length / MarkerSpacing);
        if (count <= 0)
        {
            lane.gameObject.SetActive(false);
            return;
        }
        lane.position = origin;
        lane.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        float step = length / count;
        Vector2 tileSize = laneTile.rect.size / laneTile.pixelsPerUnit;
        for (int i = 0; i < count; i++)
        {
            if (i == segments.Count)
            {
                var ink = new GameObject("Track").AddComponent<SpriteRenderer>();
                ink.transform.SetParent(lane, false);
                ink.sprite = laneTile;
                ink.sortingLayerID = boss.sortingLayerID;
                ink.sortingOrder = boss.sortingOrder - 2;
                segments.Add(ink);
            }
            var segment = segments[i];
            segment.enabled = true;
            segment.transform.localPosition = new Vector3((i + .5f) * step, 0, 0);
            segment.transform.localScale = new Vector3(step / tileSize.x, radius * 2 / tileSize.y, 1);
        }
        for (int i = count; i < segments.Count; i++) segments[i].enabled = false;
        lane.gameObject.SetActive(true);
    }
}
