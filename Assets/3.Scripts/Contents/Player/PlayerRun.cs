using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRun : MonoBehaviour
{
    PlayerStat playerStat;
    Rigidbody2D body;
    bool sprintHeld;

    public bool IsRunning => sprintHeld && !Managers.Game.isMovingTower && !TowerMoveController.IsHandling;

    void Awake()
    {
        playerStat = GetComponentInParent<PlayerStat>();
        body = playerStat.GetComponent<Rigidbody2D>();
    }

    public void RunAction(InputAction.CallbackContext context)
    {
        if (context.performed) sprintHeld = true;
        else if (context.canceled) sprintHeld = false;
    }

    void OnDisable() => sprintHeld = false;

    void Update()
    {
        if (IsRunning && playerStat.Hunger > 0 && (!FieldSession.Active || FieldBalance.HungerActive) &&
            body.linearVelocity.sqrMagnitude > .01f)
            playerStat.Hunger -= Time.deltaTime / 10;
    }
}
