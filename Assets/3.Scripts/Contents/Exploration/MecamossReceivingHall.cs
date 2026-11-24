using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public sealed class MecamossReceivingHall : MonoBehaviour
{
    [SerializeField] Tilemap floor;
    [SerializeField] Transform arrival;
    [SerializeField] LabInteraction exit;
    [SerializeField] SpriteRenderer foreman;
    [SerializeField] Animator foremanAnimator;
    [SerializeField] Transform cargo;
    [SerializeField] BoxCollider2D[] chargeHardStops;
    [SerializeField] BoxCollider2D bargeHull;

    public static MecamossReceivingHall Current { get; private set; }
    public Tilemap Floor => floor;
    public MecamossDockCharge Charge { get; private set; }
    public MecamossSlam Slam { get; private set; }
    public MecamossDiveStrike Dive => strike;
    public bool PhaseTwo { get; private set; }
    public bool Transitioning => transitionRemaining > 0;
    float transitionRemaining;
    int nextPattern;
    MecamossAlarm alarm;
    public static bool PlayerInside => Current && Managers.Game.player && Current.Contains(Managers.Game.player.transform.position);

    CircleCollider2D cargoTrigger;
    LabInteraction cargoInteraction;
    SpriteRenderer cargoArt;
    Vector2 outside;
    static readonly int Burrow = Animator.StringToHash("Base Layer.Burrow");
    Vector3 cargoStart, foremanStart;
    Coroutine introduction;
    MecamossDiveStrike strike;
    MecamossBody body;
    BoxCollider2D playerFeet;

    void Awake()
    {
        cargoStart = cargo.localPosition;
        cargoArt = cargo.GetComponent<SpriteRenderer>();
        cargoInteraction = cargo.gameObject.AddComponent<LabInteraction>();
        cargoTrigger = cargo.gameObject.AddComponent<CircleCollider2D>();
        cargoTrigger.isTrigger = true;
        cargoTrigger.radius = 1.6f;
        cargoTrigger.enabled = false;
        foremanStart = foreman.transform.localPosition;
        body = foreman.GetComponent<MecamossBody>();
        strike = new MecamossDiveStrike(foreman, body, foremanAnimator, transform);
        Charge = new MecamossDockCharge(body, foreman, foremanAnimator, chargeHardStops, bargeHull);
        Slam = new MecamossSlam(body, foreman, foremanAnimator, transform, chargeHardStops, bargeHull);
        alarm = gameObject.AddComponent<MecamossAlarm>();
        exit.Configure("공장 정문 · F 밖으로", Leave);
    }

    public bool Contains(Vector2 point) => floor.cellBounds.Contains(floor.WorldToCell(point));

    public Vector3 CameraPosition(Vector3 player, Camera camera)
    {
        Bounds bounds = floor.localBounds;
        Vector3 center = floor.transform.TransformPoint(bounds.center);
        float halfHeight = camera.orthographicSize;
        float x = Mathf.Max(0, bounds.extents.x - halfHeight * camera.aspect);
        float y = Mathf.Max(0, bounds.extents.y - halfHeight);
        return new Vector3(Mathf.Clamp(player.x, center.x - x, center.x + x),
            Mathf.Clamp(player.y, center.y - y, center.y + y), -10);
    }

    public void Enter(Vector2 returnPoint)
    {
        outside = returnPoint;
        gameObject.SetActive(true);
        Current = this;
        playerFeet = Managers.Game.player.GetComponent<BoxCollider2D>();
        body.ResetAttempt();
        RefreshCargo();
        SetSurfaced(FieldSession.State.mecamossEncountered);
        Managers.Game.player.GetComponent<PlayerController>().WarpTo(arrival.position);
        FieldHUD.Notify(FieldSession.State.mecamossEncountered ? "북서 공장 · 반송품 하역장" : "하역장 안쪽, 늪에 잠긴 반송품이 흔들린다.");
    }

    void RefreshCargo()
    {
        var state = FieldSession.State;
        cargoTrigger.enabled = state.mecamossDefeated;
        cargoArt.color = state.mecamossCargoOpened ? new Color(.62f, .58f, .52f) : Color.white;
        cargoInteraction.Configure(state.mecamossCargoOpened ? "빈 운송함" : "봉인된 운송함 · F 열기", OpenCargo);
    }

    void OpenCargo()
    {
        if (!PlayerInside || !FieldSession.State.OpenMecamossCargo()) return;
        RefreshCargo();
        cargoInteraction.ShowInteractSign();
        FieldHUD.Notify("동부 약도와 손상된 절연 장치를 챙겼다.");
        FieldHUD.ShowStory(FieldHUD.StoryPage.SouthboundManifest);
    }

    void Leave()
    {
        Managers.Game.player.GetComponent<PlayerController>().WarpTo(outside);
        gameObject.SetActive(false);
    }

    void OnDisable()
    {
        if (Current == this) Current = null;
        CancelEncounter();
    }

    void CancelEncounter()
    {
        if (introduction != null)
        {
            StopCoroutine(introduction);
            introduction = null;
        }
        transitionRemaining = 0;
        PhaseTwo = false;
        if (alarm) alarm.SetActive(false);
        strike?.Stop();
        Charge?.Stop();
        Slam?.Stop();
        if (body) body.SetExposed(false);
    }

    void Update()
    {
        if (body.Defeated)
        {
            if (!cargoTrigger.enabled) RefreshCargo();
            if (Transitioning || introduction != null || strike.Running || Charge.Stage != MecamossDockCharge.Phase.Off || Slam.Running)
            {
                CancelEncounter();
                foreman.enabled = false;
            }
            return;
        }
        if (!PlayerInside || PlayerController.isDie)
        {
            if (Transitioning || introduction != null || strike.Running || Charge.Stage != MecamossDockCharge.Phase.Off || Slam.Running)
            {
                CancelEncounter();
                body.ResetAttempt();
                SetSurfaced(FieldSession.State.mecamossEncountered);
            }
            return;
        }
        bool blocked = FieldHUD.IsOpen || Managers.Game.isCantPlay;
        foremanAnimator.speed = blocked ? 0 : 1;
        if (blocked || introduction != null || Time.deltaTime <= 0) return;
        if (Transitioning)
        {
            transitionRemaining -= Time.deltaTime;
            if (transitionRemaining <= 0) StartPattern(nextPattern);
            body.SetExposed(Transitioning || strike.Exposed || Charge.Stage != MecamossDockCharge.Phase.Off || Slam.Running);
            return;
        }
        if (strike.Running)
        {
            strike.Tick(Managers.Game.player, playerFeet);
            if (!strike.Running) CompletePattern(1);
        }
        else if (Charge.Stage != MecamossDockCharge.Phase.Off)
        {
            Charge.Tick(Managers.Game.player, playerFeet);
            if (Charge.Stage == MecamossDockCharge.Phase.Off) CompletePattern(2);
        }
        else if (Slam.Running)
        {
            Slam.Tick(Managers.Game.player, playerFeet);
            if (!Slam.Running) CompletePattern(0);
        }
        else if (Vector2.Distance(Managers.Game.player.transform.position, foreman.transform.position) <= 6)
        {
            if (FieldSession.State.mecamossEncountered) strike.Begin(true);
            else introduction = StartCoroutine(Emerge());
        }
        body.SetExposed(Transitioning || strike.Exposed || Charge.Stage != MecamossDockCharge.Phase.Off || Slam.Running);
    }

    void CompletePattern(int next)
    {
        // Never cancel the current tell, hit, or promised punish window.
        if (!PhaseTwo && body.HalfHealth && !body.Defeated)
        {
            PhaseTwo = true;
            nextPattern = next;
            transitionRemaining = 2f;
            alarm.SetActive(true);
            PlayPose(Animator.StringToHash("Base Layer.SlamStrike"));
            FieldHUD.Notify("메카모스 2페이즈 · 연속 공격에 주의");
        }
        else StartPattern(next);
    }

    void StartPattern(int next)
    {
        if (next == 0) strike.Begin(false, PhaseTwo);
        else if (next == 1) Charge.Begin(playerFeet.attachedRigidbody.position, foreman.flipX, PhaseTwo);
        else Slam.Begin(playerFeet.attachedRigidbody.position, PhaseTwo);
    }

    void SetSurfaced(bool visible)
    {
        foreman.enabled = visible && !body.Defeated;
        foreman.transform.localPosition = foremanStart;
        foremanAnimator.speed = 1;
        PlayPose(MecamossDiveStrike.Idle);
        cargo.localPosition = cargoStart + (visible ? Vector3.right * 1.1f : Vector3.zero);
    }

    void PlayPose(int state)
    {
        foremanAnimator.Play(state, 0, 0);
        foremanAnimator.Update(0);
    }

    IEnumerator Emerge()
    {
        PlayPose(Burrow);
        foreman.enabled = true;
        float pushTime = 0;
        while (true)
        {
            float push = Mathf.SmoothStep(0, 1, pushTime / .65f);
            cargo.localPosition = cargoStart + new Vector3(1.1f * push, .12f * Mathf.Sin(push * Mathf.PI), 0);
            var state = foremanAnimator.GetCurrentAnimatorStateInfo(0);
            bool resting = state.fullPathHash == MecamossDiveStrike.Idle;
            // The head breaks the water one fifth of the way through the breach.
            if (!FieldSession.State.mecamossEncountered &&
                (resting || (state.fullPathHash == MecamossDiveStrike.Breach && state.normalizedTime >= .2f)))
            {
                FieldSession.State.mecamossEncountered = true;
                FieldHUD.Notify("인수 책임자 명판에 ‘메카모스’가 새겨져 있다.");
            }
            if (resting) break;
            pushTime += Time.deltaTime * foremanAnimator.speed;
            yield return null;
        }
        cargo.localPosition = cargoStart + Vector3.right * 1.1f;
        strike.Begin(true);
        introduction = null;
    }
}
