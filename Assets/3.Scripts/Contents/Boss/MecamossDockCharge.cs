using System.Collections.Generic;
using UnityEngine;

public sealed class MecamossDockCharge
{
    public enum Phase { Off, Scraping, Rushing, Recovering }
    public Phase Stage { get; private set; }
    public Vector2 Origin { get; private set; }
    public Vector2 Direction { get; private set; }
    public BoxCollider2D Obstacle { get; private set; }
    public bool CollisionStun => Obstacle && Obstacle != bargeHull;
    public float RecoverySeconds => CollisionStun ? 2.5f : 1.5f;

    const float TellSeconds = 1f;
    const float MaxDistance = 10f;
    const float Speed = 12f;
    const float BodyRadius = 1.25f;
    const float Damage = 25f;

    readonly MecamossBody body;
    readonly MecamossChargeVisual art;
    readonly BoxCollider2D[] hardStops;
    readonly BoxCollider2D bargeHull;
    readonly List<RaycastHit2D> hits = new List<RaycastHit2D>(12);
    readonly ContactFilter2D solids = new ContactFilter2D { useTriggers = false };
    float elapsed, traveled, distance;
    bool hitPlayer, empowered;
    public int StrikeNumber { get; private set; }

    public MecamossDockCharge(MecamossBody body, SpriteRenderer sprite, Animator animator,
        BoxCollider2D[] hardStops, BoxCollider2D bargeHull)
    {
        this.body = body;
        this.hardStops = hardStops;
        this.bargeHull = bargeHull;
        art = new MecamossChargeVisual(sprite, animator);
    }

    public void Begin(Vector2 target, bool facingRight, bool empowered = false)
    {
        this.empowered = empowered;
        StrikeNumber = 1;
        Aim(target, facingRight);
    }

    void Aim(Vector2 target, bool facingRight)
    {
        Origin = body.GroundPoint;
        Vector2 aim = target - Origin;
        Direction = aim.sqrMagnitude > .001f ? aim.normalized : (facingRight ? Vector2.right : Vector2.left);
        traveled = elapsed = 0;
        hitPlayer = false;
        FindEndOfLane();
        Stage = Phase.Scraping;
        art.Begin(Origin, Direction, distance, BodyRadius);
    }

    void FindEndOfLane()
    {
        distance = MaxDistance;
        Obstacle = null;
        Physics2D.CircleCast(Origin, BodyRadius, Direction, solids, hits, MaxDistance);
        foreach (var hit in hits)
        {
            if (!(hit.collider is BoxCollider2D obstacle) || hit.distance > distance) continue;
            if (obstacle != bargeHull && System.Array.IndexOf(hardStops, obstacle) < 0) continue;
            // A dive can surface with one foot against a wall. It can still
            // charge out of that contact or along it without a false crash.
            if (hit.distance <= .001f)
            {
                Vector2 outward = Origin - obstacle.ClosestPoint(Origin);
                if (outward.sqrMagnitude < .00001f) outward = Origin - (Vector2)obstacle.bounds.center;
                if (Vector2.Dot(Direction, outward) >= 0) continue;
            }
            distance = hit.distance;
            Obstacle = obstacle;
        }
    }

    public void Tick(PlayerController player, BoxCollider2D feet)
    {
        elapsed += Time.deltaTime;
        switch (Stage)
        {
            case Phase.Scraping:
                if (elapsed >= TellSeconds)
                {
                    elapsed = 0;
                    Stage = Phase.Rushing;
                    art.Rush();
                }
                break;
            case Phase.Rushing:
                float next = Mathf.Min(distance, Speed * elapsed);
                Vector2 from = Origin + Direction * traveled;
                Vector2 to = Origin + Direction * next;
                body.transform.position += (Vector3)(to - body.GroundPoint);
                if (!hitPlayer && next > traveled && SweptIntoPlayer(from, next - traveled, feet))
                {
                    hitPlayer = true;
                    player.GetDamage(Damage);
                }
                traveled = next;
                if (traveled >= distance)
                {
                    Stage = Phase.Recovering;
                    elapsed = 0;
                    art.Recover(CollisionStun);
                }
                break;
            case Phase.Recovering:
                // Only an unobstructed first run chains. A barge still grants
                // the normal brake window; walls and crane grant the full stun.
                if (empowered && StrikeNumber == 1 && !Obstacle && elapsed >= .3f)
                {
                    StrikeNumber++;
                    Aim(feet.attachedRigidbody.position, Direction.x > 0);
                }
                else if (elapsed >= RecoverySeconds) Stop();
                break;
        }
    }

    bool SweptIntoPlayer(Vector2 start, float step, BoxCollider2D feet)
    {
        if ((feet.ClosestPoint(start) - start).sqrMagnitude <= BodyRadius * BodyRadius) return true;
        Physics2D.CircleCast(start, BodyRadius, Direction, solids, hits, step);
        foreach (var hit in hits)
            if (hit.collider == feet) return true;
        return false;
    }

    public void Stop()
    {
        if (Stage != Phase.Off) art.Stop();
        Stage = Phase.Off;
    }
}
