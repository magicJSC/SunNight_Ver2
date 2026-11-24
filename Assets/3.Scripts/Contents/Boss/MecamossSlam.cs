using UnityEngine;

public sealed class MecamossSlam
{
    public enum Phase { Off, Warning, Striking, Recovering }
    public Phase Stage { get; private set; }
    public Vector2 Origin { get; private set; }
    public Vector2 Direction { get; private set; }
    public bool Running => Stage != Phase.Off;
    static readonly int Windup = Animator.StringToHash("Base Layer.SlamWindup");
    static readonly int Strike = Animator.StringToHash("Base Layer.SlamStrike");
    static readonly int Recover = Animator.StringToHash("Base Layer.SlamRecover");
    readonly MecamossBody body;
    readonly SpriteRenderer sprite;
    readonly Animator animator;
    readonly MecamossSlamWarning warning;
    float elapsed;
    bool hitPlayer, empowered;
    public int StrikeNumber { get; private set; }

    public MecamossSlam(MecamossBody body, SpriteRenderer sprite, Animator animator,
        Transform hall, BoxCollider2D[] walls, BoxCollider2D barge)
    {
        this.body = body;
        this.sprite = sprite;
        this.animator = animator;
        warning = MecamossSlamWarning.Create(hall, walls, barge);
    }

    public void Begin(Vector2 target, bool empowered = false)
    {
        this.empowered = empowered;
        StrikeNumber = 1;
        Origin = body.GroundPoint;
        Direction = (target - Origin).normalized;
        if (Direction.sqrMagnitude < .01f) Direction = sprite.flipX ? Vector2.right : Vector2.left;
        if (Mathf.Abs(Direction.x) > .01f) sprite.flipX = Direction.x > 0;
        hitPlayer = false;
        warning.Show(Origin, Direction);
        SetPhase(Phase.Warning, Windup);
    }

    void SetPhase(Phase phase, int animation)
    {
        Stage = phase;
        elapsed = 0;
        animator.Play(animation, 0, 0);
        animator.Update(0);
    }

    public void Tick(PlayerController player, BoxCollider2D feet)
    {
        elapsed += Time.deltaTime;
        switch (Stage)
        {
            case Phase.Warning:
                warning.Telegraph(elapsed / 1.4f);
                if (elapsed >= 1.4f) SetPhase(Phase.Striking, Strike);
                break;
            case Phase.Striking:
                // The third cel is the first frame with both paws on the floor.
                if (elapsed >= .1f)
                {
                    float radius = 6f * Mathf.Clamp01((elapsed - .1f) / .2f);
                    warning.Impact(radius);
                    if (!hitPlayer && warning.Overlaps(feet, radius))
                    {
                        hitPlayer = true;
                        player.GetDamage(18);
                    }
                }
                if (elapsed >= .3f)
                {
                    warning.Hide();
                    if (empowered && StrikeNumber == 1)
                    {
                        StrikeNumber++;
                        Direction = -Direction;
                        if (Mathf.Abs(Direction.x) > .01f) sprite.flipX = Direction.x > 0;
                        hitPlayer = false;
                        warning.Show(Origin, Direction);
                        SetPhase(Phase.Warning, Windup);
                    }
                    else SetPhase(Phase.Recovering, Recover);
                }
                break;
            case Phase.Recovering:
                if (elapsed >= (empowered ? 2.5f : 2f)) Stop();
                break;
        }
    }

    public void Stop()
    {
        warning.Hide();
        Stage = Phase.Off;
    }
}
