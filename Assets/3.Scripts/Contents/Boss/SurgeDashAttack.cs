using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(Stat))]
[RequireComponent(typeof(Animator))]
public sealed class SurgeDashAttack : MonoBehaviour, IGetPlayerDamage
{
    [SerializeField] AnimationClip dash;
    [SerializeField] SpriteRenderer lane;
    LineRenderer liveLine;
    // The hand drops on cel 6 and the braking pose begins on cel 10.
    const float Launch = 19f / 24f, Brake = 25f / 24f;
    const float Radius = .46f, Damage = 18f;
    readonly ContactFilter2D solids = new ContactFilter2D { useTriggers = false };
    readonly List<RaycastHit2D> contacts = new List<RaycastHit2D>();
    SpriteRenderer art;
    Sprite idle;
    Stat health;
    BoxCollider2D hurtbox;
    Vector2 home, start, end;
    bool engaged, attacking, hit;
    float clock, rest;

    void Awake()
    {
        art = GetComponent<SpriteRenderer>();
        idle = art.sprite;
        health = GetComponent<Stat>();
        hurtbox = GetComponent<BoxCollider2D>();
        liveLine = lane.GetComponentInChildren<LineRenderer>();
        home = transform.position;
        Display(false);
    }

    void Display(bool visible)
    {
        art.enabled = hurtbox.enabled = visible;
        lane.enabled = liveLine.enabled = false;
        art.sprite = idle;
        attacking = false;
        engaged = visible;
        rest = .65f;
        transform.position = home;
    }

    void Update()
    {
        if (!FieldSession.Active || !Managers.HasInstance || !Managers.Game.player) return;
        var state = FieldSession.State;
        var player = Managers.Game.player;
        if (!SurgeFactoryHall.PlayerInside || state.surgeDefeated || PlayerController.isDie)
        {
            if (engaged || art.enabled) Display(false);
            return;
        }
        if (Managers.Game.isCantPlay || FieldHUD.IsOpen || Managers.UI.PopUIList.Count > 0) return;
        if (!engaged)
        {
            art.enabled = true;
            if (Vector2.Distance(home, player.transform.position) > 8f) return;
            Display(true);
            health.Hp = health.maxHP;
            if (!state.surgeEncountered) FieldHUD.Notify("서지의 가시에 전류가 모인다.");
            state.surgeEncountered = true;
        }
        var feet = player.GetComponent<BoxCollider2D>();
        if (!attacking)
        {
            rest -= Time.deltaTime;
            if (rest > 0) return;
            Aim(feet.bounds.center);
        }
        clock += Time.deltaTime;
        dash.SampleAnimation(gameObject, Mathf.Min(clock, dash.length));
        if (clock >= Launch)
        {
            lane.enabled = liveLine.enabled = false;
            Vector2 from = transform.position;
            Vector2 to = Vector2.Lerp(start, end, Mathf.InverseLerp(Launch, Brake, clock));
            float step = Vector2.Distance(from, to);
            if (!hit && step > 0)
            {
                Physics2D.CircleCast(from, Radius, (to-from)/step, solids, contacts, step);
                if ((feet.ClosestPoint(from)-from).sqrMagnitude <= Radius*Radius || contacts.Exists(c => c.collider == feet))
                {
                    hit = true;
                    player.GetDamage(Damage);
                }
            }
            transform.position = to;
        }
        else if (clock >= 17f/24f && lane.enabled)
        {
            lane.enabled = false;
            liveLine.enabled = true;
        }
        if (clock < dash.length) return;
        art.sprite = idle;
        attacking = false;
        rest = .55f;
    }

    void Aim(Vector2 target)
    {
        start = transform.position;
        Vector2 direction = (target-start).normalized;
        if (direction == Vector2.zero) direction = art.flipX ? Vector2.right : Vector2.left;
        float length = 7f;
        Physics2D.CircleCast(start, Radius, direction, solids, contacts, length);
        foreach (var contact in contacts)
        {
            if (!contact.transform.IsChildOf(transform.parent)) continue;
            Vector2 outward = start - contact.collider.ClosestPoint(start);
            if (contact.distance <= .001f && Vector2.Dot(direction, outward) >= 0) continue;
            length = Mathf.Min(length, Mathf.Max(0, contact.distance-.02f));
        }
        end = start + direction*length;
        liveLine.SetPosition(0, start);
        liveLine.SetPosition(1, end);
        if (Mathf.Abs(direction.x) > .01f) art.flipX = direction.x > 0;
        lane.transform.position = (start+end)*.5f;
        lane.transform.rotation = Quaternion.Euler(0,0,Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg);
        lane.transform.localScale = new Vector3(length/lane.sprite.bounds.size.x, 1, 1);
        lane.color = new Color(1,.82f,.54f);
        lane.enabled = length > .05f;
        clock = 0;
        hit = false;
        attacking = true;
    }

    public void GetDamage(float damage)
    {
        if (!engaged || damage <= 0) return;
        health.GetDamage(damage);
        if (health.Hp > 0) return;
        FieldSession.State.surgeDefeated = true;
        Display(false);
        FieldHUD.Notify("공장장 서지 처치 · 남서 송전 공장 파괴");
    }
}
