using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class TowerMoveController : MonoBehaviour
{
    const float RecoverySeconds = 2f;
    const float PlacementSeconds = 3f;
    enum Hold { None, Recover, Place }

    static TowerMoveController active;
    public static bool IsHandling => active && active.hold != Hold.None;
    readonly List<Stat> vulnerable = new();
    TurretController[] attachedTurrets;
    Hold hold;
    float started;
    int shownPercent;
    bool placePressed;
    Vector2Int installCell;
    TowerPlacement placement;
    Tilemap preview;
    GameObject previewGrid;

    void Start()
    {
        active = this;
        Managers.Game.isMovingTower = false;
    }

    void OnDisable()
    {
        CancelHold();
        if (preview) preview.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (active == this) active = null;
        if (previewGrid) Destroy(previewGrid);
    }

    bool CanUse()
    {
        var game = Managers.Game;
        var clock = game.timeController;
        bool night = clock && clock.isActiveAndEnabled && clock.day > 0 && clock.timeType == TimeController.TimeType.Night;
        return game.player && game.tower && game.canMoveTower && !game.isCantPlay && !night &&
            game.mouse && game.mouse.CursorType != Define.CursorType.Drag &&
            !PlayerController.isDie && !FieldHUD.IsOpen && !FieldLaboratory.PlayerInside &&
            FieldFoxtonBattle.Active == null && Managers.UI.PopUIList.Count == 0 && Time.timeScale > 0;
    }

    static bool MovingInput()
    {
        var keys = Keyboard.current;
        return keys != null && (keys.wKey.isPressed || keys.sKey.isPressed || keys.aKey.isPressed || keys.dKey.isPressed ||
            keys.upArrowKey.isPressed || keys.downArrowKey.isPressed || keys.leftArrowKey.isPressed || keys.rightArrowKey.isPressed);
    }

    void Update()
    {
        var keys = Keyboard.current;
        var mouse = Mouse.current;
        bool startPlace = placePressed || (mouse != null && mouse.leftButton.wasPressedThisFrame);
        placePressed = false;
        if (!CanUse())
        {
            CancelHold();
            if (preview) preview.gameObject.SetActive(false);
            return;
        }
        bool carrying = Managers.Game.isMovingTower;
        if (!carrying && keys != null && keys.vKey.wasPressedThisFrame) BeginRecovery();
        bool show = carrying && Managers.Inven.choiceIndex == InvenManager.BaseSlotIndex &&
            Managers.Game.mouse.CursorType != Define.CursorType.UI && mouse != null && Camera.main;
        if (preview && preview.gameObject.activeSelf != show) preview.gameObject.SetActive(show);

        Vector2Int cell = default;
        Vector2 playerPosition = default;
        bool valid = false;
        if (show)
        {
            cell = Vector2Int.RoundToInt(Camera.main.ScreenToWorldPoint(mouse.position.ReadValue()));
            valid = placement.Manual(cell, out playerPosition);
            previewGrid.transform.position = (Vector2)cell;
            preview.color = valid ? new Color(.7f, 1, .6f, .5f) : new Color(1, .25f, .25f, .5f);
            if (startPlace && valid && hold == Hold.None && !MovingInput())
            {
                installCell = cell;
                StartHold(Hold.Place);
            }
        }

        if (hold == Hold.None) return;
        bool recovering = hold == Hold.Recover;
        bool interrupted = recovering ?
            keys == null || !keys.vKey.isPressed || (mouse != null && mouse.leftButton.isPressed) || !NearTower() :
            !show || !mouse.leftButton.isPressed || !valid || cell != installCell;
        if (interrupted || MovingInput())
        {
            CancelHold();
            return;
        }
        float duration = recovering ? RecoverySeconds : PlacementSeconds;
        float progress = (Time.time - started) / duration;
        if (progress < 1)
        {
            int percent = Mathf.FloorToInt(progress * 10) * 10;
            if (percent != shownPercent)
            {
                FieldHUD.Notify((recovering ? "거점 회수 중 · V 유지 " : "거점 설치 중 · 좌클릭 유지 ") + percent + "%");
                shownPercent = percent;
            }
            return;
        }

        FinishHold(recovering);
        if (recovering) Pack();
        else Deploy(cell, playerPosition);
    }

    bool NearTower() => Vector2.Distance(Managers.Game.player.transform.position, Managers.Game.tower.transform.position) <= 2f;

    void BeginRecovery()
    {
        if (hold != Hold.None || Managers.Game.isMovingTower || !CanUse() || MovingInput()) return;
        if (!NearTower()) { FieldHUD.Notify("거점 가까이에서 V를 2초간 눌러 회수하세요."); return; }
        StartHold(Hold.Recover);
    }

    void StartHold(Hold next)
    {
        hold = next;
        started = Time.time;
        shownPercent = -1;
        vulnerable.Add(Managers.Game.player.GetComponent<Stat>());
        if (next == Hold.Recover)
        {
            vulnerable.AddRange(Managers.Game.tower.GetComponentsInChildren<Stat>());
            attachedTurrets = Managers.Game.tower.GetComponentsInChildren<TurretController>();
            foreach (var turret in attachedTurrets) turret.PauseForTransport();
        }
        foreach (var stat in vulnerable) stat.damagedEvent += CancelHold;
    }

    void CancelHold() => FinishHold(false);

    void FinishHold(bool packed)
    {
        if (hold == Hold.None) return;
        if (hold == Hold.Recover && !packed) ResumeTurrets();
        foreach (var stat in vulnerable)
            if (stat) stat.damagedEvent -= CancelHold;
        vulnerable.Clear();
        hold = Hold.None;
    }

    void ResumeTurrets()
    {
        if (attachedTurrets == null) return;
        foreach (var turret in attachedTurrets)
            if (turret) turret.ResumeAfterTransport();
        attachedTurrets = null;
    }

    void Pack()
    {
        var game = Managers.Game;
        if (!previewGrid)
        {
            previewGrid = new GameObject("거점 설치 미리보기", typeof(Grid));
            preview = Instantiate(MapManager.tower, previewGrid.transform, false);
        }
        preview.ClearAllTiles();
        placement = new TowerPlacement(preview);
        game.isMovingTower = true;
        game.tower.gameObject.SetActive(false);
        Managers.Inven.hotBarUI.SelectBaseSlot();
        FieldHUD.Notify("거점 회수 완료 · 5번을 선택하고 초록색 위치에서 좌클릭을 3초간 유지하세요.");
    }

    void Deploy(Vector2Int cell, Vector2 playerPosition)
    {
        var game = Managers.Game;
        CancelHold();
        preview.gameObject.SetActive(false);
        if ((Vector2)game.player.transform.position != playerPosition)
        {
            var body = game.player.GetComponent<Rigidbody2D>();
            body.position = playerPosition;
            game.player.transform.position = playerPosition;
        }
        game.tower.transform.position = (Vector2)cell;
        game.isMovingTower = false;
        game.tower.gameObject.SetActive(true);
        ResumeTurrets();
        Physics2D.SyncTransforms();
        Managers.Map.SetCanBuildTile();
        // Only the builder's availability changed. Preserve a weapon swing or inventory drag at dusk.
        var selected = Managers.Inven.currentItemUI;
        if (selected && selected.slotInfo?.itemInfo?.itemType == Define.ItemType.Building &&
            Managers.UI.PopUIList.Count == 0 && game.mouse.CursorType != Define.CursorType.Drag)
            Managers.Inven.CheckHotBarChoice();
        FieldHUD.Notify("거점 설치 완료");
    }

    // The Player prefab also routes these controls through its InputAction UnityEvents.
    public void TryMoveTower(InputAction.CallbackContext context)
    {
        if (context.performed && active == this) BeginRecovery();
    }

    public void TryBuildTowerAction(InputAction.CallbackContext context)
    {
        if (context.performed) placePressed = true;
    }

    public static bool TryDeployForNight()
    {
        if (active) active.CancelHold();
        if (!Managers.Game.isMovingTower) return true;
        if (!active || !active.isActiveAndEnabled || active.placement == null || FieldLaboratory.PlayerInside) return false;
        if (!active.placement.Nearest(out var position, out var exit)) return false;
        active.Deploy(position, exit);
        return true;
    }
}
