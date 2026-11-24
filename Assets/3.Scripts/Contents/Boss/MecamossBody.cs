using UnityEngine;

[RequireComponent(typeof(Stat), typeof(BoxCollider2D), typeof(SpriteRenderer))]
public sealed class MecamossBody : MonoBehaviour, IGetPlayerDamage
{
    Stat health;
    BoxCollider2D hurtbox;
    SpriteRenderer art;
    Color restColor;
    float flashUntil;

    public bool HalfHealth => health.Hp <= health.maxHP * .5f;
    public bool Defeated => FieldSession.State.mecamossDefeated;
    // Waterline at row 80 of the 112 px cel, 24 px below its pivot at 20 PPU.
    public Vector2 GroundPoint => transform.TransformPoint(0, -1.2f, 0);

    void Awake()
    {
        health = GetComponent<Stat>();
        hurtbox = GetComponent<BoxCollider2D>();
        art = GetComponent<SpriteRenderer>();
        restColor = art.color;
        health.damagedEvent += Flash;
        health.dieEvent += Defeat;
    }

    public void ResetAttempt()
    {
        SetExposed(false);
        health.enabled = !Defeated;
        health.Hp = Defeated ? 0 : health.maxHP;
        ClearFlash();
    }

    public void SetExposed(bool exposed)
    {
        bool canBeHit = exposed && !Defeated && isActiveAndEnabled;
        if (hurtbox.enabled != canBeHit) hurtbox.enabled = canBeHit;
    }

    public void GetDamage(float damage)
    {
        if (!hurtbox.enabled || !art.enabled || Defeated || damage <= 0) return;
        health.GetDamage(damage);
    }

    void Flash()
    {
        art.color = restColor * new Color(1f, .57f, .44f, 1f);
        flashUntil = Time.time + .09f;
    }

    void LateUpdate()
    {
        if (flashUntil > 0 && Time.time >= flashUntil) ClearFlash();
    }

    void ClearFlash()
    {
        flashUntil = 0;
        art.color = restColor;
    }

    void Defeat()
    {
        if (Defeated) return;
        FieldSession.State.mecamossDefeated = true;
        SetExposed(false);
        art.enabled = false;
        FieldHUD.Notify("공장장 메카모스 처치 · 북서 늪지 공장 파괴");
    }

    void OnDisable()
    {
        hurtbox.enabled = false;
        ClearFlash();
    }

    void OnDestroy()
    {
        health.damagedEvent -= Flash;
        health.dieEvent -= Defeat;
    }
}
