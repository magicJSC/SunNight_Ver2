using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

// Run in Play Mode on a disposable session. Every case reloads the tracked Day 2 camp fixture.
public static class BaseCarrySmoke
{
    static PlayerController Player => Managers.Game.player;
    static Vector2 Position => Player.transform.position;
    static Vector2 BasePosition => Managers.Game.tower.transform.position;

    static void Check(bool passed, string label)
    {
        if (!passed) throw new InvalidOperationException(label);
    }

    static async Task Until(Func<bool> condition, string label, float timeout = 8)
    {
        float end = Time.realtimeSinceStartup + timeout;
        while (!condition())
        {
            if (Time.realtimeSinceStartup > end) throw new TimeoutException(label);
            await Task.Delay(40);
        }
    }

    static void PlacePlayer(Vector2 point)
    {
        var body = Player.GetComponent<Rigidbody2D>();
        body.linearVelocity = Vector2.zero;
        body.position = point;
        Player.transform.position = point;
        Physics2D.SyncTransforms();
    }

    static void Keys(params Key[] keys)
    {
        InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(keys));
    }

    static async Task Press(Key key)
    {
        Keys(key);
        await Task.Delay(160);
        Keys();
        await Task.Delay(160);
    }

    static string[] Facilities()
    {
        return FieldCheckpoint.Capture().buildings.Select(b => $"{b.x},{b.y}:{b.item}:{b.hp}").OrderBy(s => s).ToArray();
    }

    static bool PreviewMatches(Vector2Int cell, bool valid)
    {
        var grid = GameObject.Find("거점 설치 미리보기");
        var preview = grid ? grid.GetComponentInChildren<Tilemap>() : null;
        if (!preview || Vector2.Distance(grid.transform.position, cell) >= .01f) return false;
        return valid ? preview.color.g > preview.color.r : preview.color.r > preview.color.g;
    }

    public static async Task<string> Run()
    {
        var checks = new List<string>();
        foreach (var scenario in new[] { "stationary", "carry", "fallback", "retry", "manual" })
            checks.Add(await RunCase(scenario));
        return string.Join("\n", checks);
    }

    public static Task<string> Stationary() => RunCase("stationary");
    public static Task<string> Carry() => RunCase("carry");
    public static Task<string> Fallback() => RunCase("fallback");
    public static Task<string> Retry() => RunCase("retry");
    public static Task<string> Manual() => RunCase("manual");

    static async Task<string> RunCase(string scenario)
    {
        var background = InputSystem.settings.backgroundBehavior;
        var editorInput = InputSystem.settings.editorInputBehaviorInPlayMode;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        try
        {
            string path = Path.GetFullPath("docs/level-design/v03-postnight-final-pre-factory.json");
            FieldCheckpoint.RequestResume(path);
            SceneManager.LoadScene("TutorialScene");
            await Task.Delay(150);
            await Until(() => !FieldSession.ResumeRequested && FieldInventory.Ready && GameObject.Find("CentralCampWorkbench") != null,
                "Waiting for earned Day 2 fixture", 30);
            var clock = Managers.Game.timeController;
            clock.timeSpeed = 0;
            Vector2 start = BasePosition;
            Check(clock.day == 2 && FieldSession.State.firstNightCleared, "Expected earned Day 2 fixture");
            var attached = Facilities();
            Check(attached.Length == 9, "Missing earned cannons/walls before test");
            var hotbar = Managers.Inven.hotBarUI;
            var slot = hotbar.slotList[4];
            var icon = slot.transform.Find("CarriedBase").GetComponent<Image>();
            Check(slot.itemUI.slotInfo.itemInfo == null && !icon.enabled, "Installed base must leave a reserved empty slot");
            Check(FieldInventory.Count(ExplorationRuntime.Instance.content.Item("HpPotion")) ==
                FieldCheckpoint.Read(ExplorationRuntime.Instance.content, path).slots.Where(s => s.item == "HpPotion").Sum(s => s.count),
                "Legacy slot-five potion lost or duplicated during restore");
            PlacePlayer(start + Vector2.down * 1.65f);
            if (scenario != "stationary")
            {
                Keys(Key.V);
                await Until(() => Managers.Game.isMovingTower && !Managers.Game.isCantPlay, "V failed to complete packing");
                Keys();
                await Until(() => icon.enabled && icon.sprite, "Missing slot-five carry icon");
                await Press(Key.Digit5);
                Check(Managers.Inven.choiceIndex == 4, "Slot five not selectable");
                await Press(Key.Digit1);
                Check(Managers.Inven.choiceIndex == 0 && Managers.Game.weapon != null && Managers.Game.isMovingTower,
                    "Could not equip the fixture bat while carrying");
                var beforeWalking = Position;
                Keys(Key.D);
                await Until(() => Position.x - beforeWalking.x >= 3, "Movement blocked while carrying");
                Keys();
                await Task.Delay(150);
            }

            if (scenario == "manual")
            {
                await Press(Key.Digit5);
                var target = Vector2Int.RoundToInt(Position + Vector2.down * 1.25f);
                var aim = new MouseState { position = Camera.main.WorldToScreenPoint((Vector2)target) };
                InputSystem.QueueStateEvent(Mouse.current, aim);
                await Until(() => PreviewMatches(target, true), "No green preview at camp");
                InputSystem.QueueStateEvent(Mouse.current, aim.WithButton(MouseButton.Left));
                await Until(() => !Managers.Game.isMovingTower, "Manual install did not complete");
                InputSystem.QueueStateEvent(Mouse.current, aim);
                Check(Vector2.Distance(BasePosition, target) < .1f && attached.SequenceEqual(Facilities()),
                    "Manual installation moved or damaged the layout");
                return "PASS manual: green preview, hold to install, original damaged layout restored";
            }

            var foot = Vector2Int.RoundToInt(Position);
            PlacePlayer(foot);
            if (scenario == "fallback")
            {
                var blocker = ScriptableObject.CreateInstance<Tile>();
                blocker.colliderType = Tile.ColliderType.None;
                MapManager.cantBuild.SetTile(new Vector3Int(foot.x, foot.y, 0), blocker);
                await Press(Key.Digit5);
                var aim = new MouseState { position = Camera.main.WorldToScreenPoint((Vector2)foot) };
                InputSystem.QueueStateEvent(Mouse.current, aim);
                await Until(() => PreviewMatches(foot, false), "Blocked cell did not show a red preview");
                await Press(Key.Digit1);
            }
            BoxCollider2D obstruction = null;
            if (scenario == "retry")
            {
                obstruction = new GameObject("QA blocked ground").AddComponent<BoxCollider2D>();
                obstruction.transform.position = (Vector2)foot;
                obstruction.size = Vector2.one * 10;
                foreach (var collider in Player.GetComponentsInChildren<Collider2D>())
                    Physics2D.IgnoreCollision(collider, obstruction);
                Physics2D.SyncTransforms();
            }
            int nights = 0;
            bool installedBeforeWave = false;
            clock.nightEvent += () => { nights++; installedBeforeWave = !Managers.Game.isMovingTower; };
            clock.RestoreClock(2, 1289.8f);
            clock.timeSpeed = 2;
            if (scenario == "retry")
            {
                await Until(() => clock.IsNightPlacementPending, "Blocked ground did not delay placement");
                await Task.Delay(700);
                Check(clock.TimeAmount == 1290 && nights == 0 && Managers.Game.isMovingTower,
                    "Clock/wave advanced without valid ground");
                obstruction.enabled = false;
                UnityEngine.Object.Destroy(obstruction.gameObject);
            }
            await Until(() => nights == 1, "Missing night transition");
            await Until(() => !icon.enabled, "Pack icon still visible after installation");
            Check(installedBeforeWave && !clock.IsNightPlacementPending, "Night event fired before placement completed");
            if (scenario == "stationary")
                Check(Vector2.Distance(BasePosition, start) < .1f, "Parked base moved at dusk");
            else if (scenario == "fallback")
                Check(Mathf.Abs(Vector2.Distance(BasePosition, foot) - 1f) < .01f, $"Expected adjacent valid anchor from {foot}, got {BasePosition}");
            else
                Check(Vector2.Distance(BasePosition, foot) < .1f, $"Expected foot anchor {foot}, got {BasePosition}");
            Check(attached.SequenceEqual(Facilities()), "Attached facilities or damage changed");
            await Until(() => UnityEngine.Object.FindObjectsByType<MonsterController>()
                .Any(m => m.targetType == MonsterController.TargetType.Tower && Vector2.Distance(m.transform.position, BasePosition) < 13),
                "No defense enemy spawned around the deployed base", 20);
            Check(nights == 1, "Duplicate night transition");
            return $"PASS {scenario}: one wave after placement at {BasePosition}, nine facilities preserved";
        }
        finally
        {
            Keys();
            if (Mouse.current != null)
                InputSystem.QueueStateEvent(Mouse.current, new MouseState { position = Mouse.current.position.ReadValue() });
            InputSystem.settings.backgroundBehavior = background;
            InputSystem.settings.editorInputBehaviorInPlayMode = editorInput;
        }
    }
}
