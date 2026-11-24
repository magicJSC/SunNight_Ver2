using UnityEngine;

// Drives one attack from the foreman's existing one-shot animation clips.
// The player's body is sampled once for the target and once for the hit.
public sealed class MecamossDiveStrike
{
    const float WarningSeconds = 1.2f;
    const float Radius = 2f;
    const float Damage = 22f;
    const float RestSeconds = 2f;
    // Breach cel 3 shows the jaw fully breaking the surface, 0.20s into the clip.
    const float JawBreakSeconds = .2f;
    static readonly int Dive = Animator.StringToHash("Base Layer.Dive");
    internal static readonly int Idle = Animator.StringToHash("Base Layer.Idle");
    internal static readonly int Breach = Animator.StringToHash("Base Layer.Breach");

    public enum Phase { Off, Rest, Diving, Warning, Rising, Landing, Recovery }
    readonly SpriteRenderer boss;
    readonly MecamossBody body;
    readonly Animator animator;
    readonly MecamossBreachWarning warning;
    Phase phase;
    public Phase Stage => phase;
    public int StrikeNumber { get; private set; }
    bool empowered;
    float elapsed;
    Vector2 target;

    public bool Running => phase != Phase.Off;
    public bool Exposed => phase == Phase.Rest || phase == Phase.Landing || phase == Phase.Recovery;

    public MecamossDiveStrike(SpriteRenderer boss, MecamossBody body, Animator animator, Transform hall)
    {
        this.boss = boss;
        this.body = body;
        this.animator = animator;
        warning = MecamossBreachWarning.Create(hall);
    }

    public void Begin(bool allowEntryDelay, bool empowered = false)
    {
        this.empowered = empowered;
        StrikeNumber = 1;
        elapsed = 0;
        if (allowEntryDelay) phase = Phase.Rest;
        else StartDive();
    }

    void StartDive()
    {
        elapsed = 0;
        animator.Play(Dive, 0, 0);
        animator.Update(0);
        phase = Phase.Diving;
    }

    public void Stop()
    {
        phase = Phase.Off;
        warning.Hide();
    }

    public void Tick(PlayerController player, BoxCollider2D feet)
    {
        elapsed += Time.deltaTime;
        var state = animator.GetCurrentAnimatorStateInfo(0);
        switch (phase)
        {
            case Phase.Rest:
                if (elapsed >= RestSeconds) StartDive();
                break;
            case Phase.Diving:
                // The controller transitions directly from Dive to Burrow.
                if (state.fullPathHash != Dive)
                {
                    target = feet.attachedRigidbody.position;
                    boss.enabled = false;
                    warning.Show(target, Radius);
                    elapsed = 0;
                    phase = Phase.Warning;
                }
                break;
            case Phase.Warning:
                warning.SetProgress(elapsed / WarningSeconds);
                if (elapsed >= WarningSeconds - JawBreakSeconds)
                {
                    // Register the sprite's waterline (not the texture center)
                    // to the very same world point used by the marker and hit.
                    boss.transform.position += (Vector3)(target - body.GroundPoint);
                    animator.Play(Breach, 0, 0);
                    animator.Update(0);
                    boss.enabled = true;
                    phase = Phase.Rising;
                }
                break;
            case Phase.Rising:
                warning.SetProgress(elapsed / WarningSeconds);
                if (elapsed >= WarningSeconds &&
                    (state.fullPathHash == Idle || state.normalizedTime * state.length >= JawBreakSeconds))
                {
                    warning.Hide();
                    phase = Phase.Landing;
                    if ((feet.ClosestPoint(target) - target).sqrMagnitude <= Radius * Radius)
                        player.GetDamage(Damage);
                }
                break;
            case Phase.Landing:
                if (state.fullPathHash == Idle)
                {
                    if (empowered && StrikeNumber == 1)
                    {
                        StrikeNumber++;
                        StartDive();
                    }
                    else
                    {
                        elapsed = 0;
                        phase = Phase.Recovery;
                    }
                }
                break;
            case Phase.Recovery:
                if (elapsed >= (empowered ? 2.5f : RestSeconds)) phase = Phase.Off;
                break;
        }
    }
}
