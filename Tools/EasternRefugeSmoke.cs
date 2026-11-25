using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Run through Unity Pipeline in a disposable Play Mode session.
public static class EasternRefugeSmoke
{
    static Keyboard keyboard;
    static Mouse mouse;
    static string Hud(string path) => GameObject.Find("ExplorationHUD/" + path)?.GetComponent<Text>()?.text;
    static void Check(bool condition, string label)
    {
        if (!condition) throw new Exception(label);
    }
    static async Task Wait(Func<bool> ready, string label)
    {
        float until = Time.realtimeSinceStartup + 20;
        while (!ready())
        {
            if (Time.realtimeSinceStartup > until) throw new Exception("Timeout: " + label + " / " + Hud("Interaction"));
            await Task.Delay(40);
        }
    }
    static async Task Press(Key key)
    {
        keyboard.MakeCurrent();
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(key));
        await Task.Delay(120);
        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        await Task.Delay(150);
    }
    static async Task Load(string file)
    {
        FieldCheckpoint.RequestResume(file);
        SceneManager.LoadScene("TutorialScene");
        await Wait(() => !FieldSession.ResumeRequested && GameObject.Find("CentralCampWorkbench") &&
            GameObject.Find("RefugeParts") && FieldInventory.Ready, "surface and workshop");
        Managers.Game.player.GetComponent<PlayerInput>().SwitchCurrentControlScheme("Keyboard&Mouse", keyboard, mouse);
    }
    static async Task Collect(string name)
    {
        var reward = GameObject.Find(name).GetComponent<FieldReward>();
        await Walk((Vector2)reward.transform.position + Vector2.down * .6f);
        await Wait(() => Hud("Interaction")?.Contains("폐정비소") == true, "workshop interaction");
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.F));
        try { await Wait(() => reward.Empty, "collected " + name); }
        finally { InputSystem.QueueStateEvent(keyboard, new KeyboardState()); }
        await Task.Delay(160);
    }
    static async Task Walk(Vector2 goal)
    {
        var body = Managers.Game.player.GetComponent<Rigidbody2D>();
        var ground = Managers.Game.grid.transform.Find("Tilemap").GetComponent<UnityEngine.Tilemaps.Tilemap>();
        float until = Time.realtimeSinceStartup + 22;
        while (Vector2.Distance(goal, body.position) > .25f)
        {
            var delta = goal - body.position;
            var keys = new System.Collections.Generic.List<Key>(2);
            if (Mathf.Abs(delta.x) > .13f) keys.Add(delta.x > 0 ? Key.D : Key.A);
            if (Mathf.Abs(delta.y) > .13f) keys.Add(delta.y > 0 ? Key.W : Key.S);
            keyboard.MakeCurrent();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys.ToArray()));
            Check(ground.HasTile(ground.WorldToCell(body.position)), "walked off terrain at " + body.position);
            Check(Time.realtimeSinceStartup < until, "blocked walking from " + body.position + " to " + goal);
            await Task.Delay(18);
        }
        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        await Task.Delay(150);
    }

    public static async Task<string> Run()
    {
        Check(Application.isPlaying, "Run in Play Mode");
        var oldKeyboard = Keyboard.current;
        var oldMouse = Mouse.current;
        var settings = InputSystem.settings;
        var background = settings.backgroundBehavior;
        var focus = settings.editorInputBehaviorInPlayMode;
        string folder = Directory.CreateDirectory("Logs/EasternRefugeSmoke-" + Guid.NewGuid().ToString("N")).FullName;
        string file = Path.Combine(folder, "checkpoint.json");
        try
        {
            settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            keyboard = InputSystem.AddDevice<Keyboard>();
            mouse = InputSystem.AddDevice<Mouse>();
            var gate = EasternRefuge.Entrance;
            var seed = new FieldSave { state = new ExplorationState { firstNightCleared = true, foxtonFactoryDestroyed = true,
                foxtonReturnSlipCollected = true, northwestFactoryGateDiscovered = true, mecamossDefeated = true,
                mecamossCargoOpened = true, hasDamagedInsulator = true, basementMapReceived = true, sunNightJournalRead = true },
                playerX = gate.x - 14, playerY = gate.y - 15, towerX = 70, towerY = -70, hp = 91, towerHp = 500,
                day = 2, time = 670, hunger = 90, slots = Enumerable.Range(0,29).Select(i => new FieldSlot()).ToArray() };
            File.WriteAllText(file, JsonConvert.SerializeObject(seed));
            await Load(file);
            await Wait(() => UnityEngine.Object.FindAnyObjectByType<EasternRefuge>(), "camp spawn");
            await Press(Key.M);
            Check(GameObject.Find("EastSearchArea") && !GameObject.Find("EastRefugeGate"), "search area before discovery");
            await Press(Key.M);
            await Walk(gate + new Vector2(-5,-10));
            await Walk(gate + Vector2.down * 4);
            Check(!FieldSession.State.eastRefugeDiscovered, "discovery needs gate approach");
            ScreenCapture.CaptureScreenshot(Path.Combine(folder,"approach.png"));
            await Task.Delay(200);
            await Walk(gate);
            Check(FieldSession.State.eastRefugeDiscovered && Hud("MissionBanner/StoryObjective") == "정비사에게 절연 장치 보여주기", "discover gate");
            await Press(Key.M);
            Check(GameObject.Find("EastRefugeGate") && !GameObject.Find("EastSearchArea"), "confirmed entrance on map");
            ScreenCapture.CaptureScreenshot(Path.Combine(folder,"map.png"));
            await Task.Delay(200);
            await Press(Key.M);
            await Walk(gate + new Vector2(.5f,3.5f));
            await Wait(() => Hud("Interaction")?.Contains("정비사") == true, "mechanic prompt");
            await Press(Key.F);
            Check(FieldSession.State.insulatorRepairRequested && FieldSession.State.hasDamagedInsulator &&
                Hud("MissionBanner/StoryObjective") == "절연 장치 수리하기", "repair quest accepted without consuming device");
            ScreenCapture.CaptureScreenshot(Path.Combine(folder,"mechanic.png"));
            await Task.Delay(200);
            await Press(Key.F);
            Check(Hud("Notice")?.Contains("고철 0/2") == true, "empty bag shows recipe");
            FieldCheckpoint.Save(file);
            await Load(file);
            await Wait(() => Hud("MissionBanner/StoryObjective") == "절연 장치 수리하기", "restored quest");
            Check(FieldSession.State.eastRefugeDiscovered && FieldSession.State.hasDamagedInsulator, "restored discovery and device");

            var materials = ExplorationRuntime.Instance.content;
            await Walk(gate + Vector2.down * 3);
            await Collect("RefugeScrap");
            Check(FieldInventory.Count(materials.scrap) == 2 && FieldInventory.Count(materials.parts) == 0, "scrap from workbench");
            await Walk(gate + Vector2.down * 3);
            await Walk(gate + new Vector2(.5f,3.5f));
            await Press(Key.F);
            Check(FieldSession.State.hasDamagedInsulator && FieldInventory.Count(materials.scrap) == 2 &&
                Hud("Notice")?.Contains("정비 부품 0/1") == true, "partial delivery leaves scrap intact");
            await Walk(gate + Vector2.down * 3);
            await Collect("RefugeParts");
            Check(FieldInventory.Count(materials.parts) == 1, "part from workshop box");
            ScreenCapture.CaptureScreenshot(Path.Combine(folder,"materials.png"));
            await Task.Delay(200);
            await Walk(gate + Vector2.down * 3);
            await Walk(gate + new Vector2(.5f,3.5f));
            await Press(Key.F);
            Check(FieldSession.State.hasRepairedInsulator && !FieldSession.State.hasDamagedInsulator &&
                FieldInventory.Count(materials.scrap) == 0 && FieldInventory.Count(materials.parts) == 0 &&
                Hud("MissionBanner/StoryObjective") == "남서쪽 서지 공장 찾기", "repair and Surge objective");
            ScreenCapture.CaptureScreenshot(Path.Combine(folder,"repaired.png"));
            await Task.Delay(200);
            await Press(Key.F);
            Check(Hud("Notice")?.Contains("다 고쳤어") == true, "no repeat repair");
            FieldCheckpoint.Save(file);
            await Load(file);
            await Wait(() => Hud("MissionBanner/StoryObjective") == "남서쪽 서지 공장 찾기", "restored repair");
            Check(FieldSession.State.hasRepairedInsulator && GameObject.Find("RefugeParts").GetComponent<FieldReward>().Empty &&
                GameObject.Find("RefugeScrap").GetComponent<FieldReward>().Empty, "saved device and claims");

            seed.state.hasDamagedInsulator = false;
            seed.state.mecamossCargoOpened = false;
            seed.state.mecamossDefeated = false;
            seed.playerX = gate.x; seed.playerY = gate.y - 3;
            File.WriteAllText(file, JsonConvert.SerializeObject(seed));
            await Load(file);
            await Walk(gate);
            await Walk(gate + new Vector2(.5f,3.5f));
            await Wait(() => Hud("Interaction")?.Contains("정비사") == true, "early visit prompt");
            await Press(Key.F);
            Check(FieldSession.State.eastRefugeDiscovered && !FieldSession.State.insulatorRepairRequested &&
                Hud("Notice")?.Contains("가져와") == true, "early visit discovery without quest bypass");
            return "PASS: walked, gathered scrap 2 + part 1, partial delivery, repaired, Surge objective, repeat, save/load, early visit / " + folder;
        }
        finally
        {
            if (keyboard != null) InputSystem.RemoveDevice(keyboard);
            if (mouse != null) InputSystem.RemoveDevice(mouse);
            if (oldKeyboard != null && oldKeyboard.added) oldKeyboard.MakeCurrent();
            if (oldMouse != null && oldMouse.added) oldMouse.MakeCurrent();
            settings.backgroundBehavior = background;
            settings.editorInputBehaviorInPlayMode = focus;
        }
    }
}
