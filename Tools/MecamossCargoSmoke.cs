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
public static class MecamossCargoSmoke
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
        await Wait(() => !FieldSession.ResumeRequested && GameObject.Find("CentralCampWorkbench") && FieldInventory.Ready, "surface");
        Managers.Game.player.GetComponent<PlayerInput>().SwitchCurrentControlScheme("Keyboard&Mouse", keyboard, mouse);
    }
    static async Task Enter()
    {
        var player = Managers.Game.player.GetComponent<PlayerController>();
        player.WarpTo(new Vector2(-222, 143));
        await Task.Delay(200);
        player.WarpTo(new Vector2(-222, 149.5f));
        await Wait(() => Hud("Interaction")?.Contains("북서 공장 정문") == true, "gate prompt");
        await Press(Key.F);
        await Wait(() => MecamossReceivingHall.PlayerInside, "enter");
    }
    static async Task Leave()
    {
        Managers.Game.player.GetComponent<PlayerController>().WarpTo(MecamossReceivingHall.Current.transform.Find("SouthExit").position);
        await Wait(() => Hud("Interaction")?.Contains("밖으로") == true, "exit prompt");
        await Press(Key.F);
        await Wait(() => !MecamossReceivingHall.PlayerInside, "exit");
    }
    public static async Task<string> Run()
    {
        Check(Application.isPlaying, "Run in Play Mode");
        var oldKeyboard = Keyboard.current;
        var oldMouse = Mouse.current;
        var settings = InputSystem.settings;
        var background = settings.backgroundBehavior;
        var focus = settings.editorInputBehaviorInPlayMode;
        string folder = Directory.CreateDirectory("Logs/CargoSmoke-" + Guid.NewGuid().ToString("N")).FullName;
        string file = Path.Combine(folder, "checkpoint.json");
        try
        {
            settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            keyboard = InputSystem.AddDevice<Keyboard>();
            mouse = InputSystem.AddDevice<Mouse>();
            var state = new ExplorationState { firstNightCleared = true, foxtonFactoryDestroyed = true, foxtonReturnSlipCollected = true,
                basementMapReceived = true, sunNightJournalRead = true, mecamossEncountered = true };
            var seed = new FieldSave { state = state, playerX = -222, playerY = 145, towerX = 70, towerY = -70,
                hp = 91, towerHp = 500, hunger = 100, day = 2, time = 670, slots = Enumerable.Range(0, 29)
                    .Select(i => i == InvenManager.BaseSlotIndex ? new FieldSlot() : new FieldSlot { item = "Bat", count = 1 }).ToArray() };
            File.WriteAllText(file, JsonConvert.SerializeObject(seed));
            await Load(file);
            await Enter();
            var hall = MecamossReceivingHall.Current;
            var crate = hall.transform.Find("ReturnedRobotCrate");
            var player = Managers.Game.player.GetComponent<PlayerController>();
            player.WarpTo((Vector2)crate.position + Vector2.down * .5f);
            await Press(Key.F);
            Check(!FieldSession.State.mecamossCargoOpened && !crate.GetComponent<CircleCollider2D>().enabled, "locked before defeat");
            hall.GetComponentInChildren<MecamossBody>().GetComponent<Stat>().GetDamage(9999);
            await Wait(() => Hud("MissionBanner/StoryObjective") == "하역장의 운송함 조사하기", "first objective");
            await Leave();
            FieldCheckpoint.Save(file);
            await Load(file);
            await Enter();
            crate = MecamossReceivingHall.Current.transform.Find("ReturnedRobotCrate");
            Managers.Game.player.GetComponent<PlayerController>().WarpTo((Vector2)crate.position + Vector2.down * .5f);
            await Wait(() => Hud("Interaction")?.Contains("F 열기") == true, "unopened crate after load");
            string inventory = JsonConvert.SerializeObject(FieldCheckpoint.CaptureInventory());
            await Press(Key.F);
            Check(Hud("StoryRecord/Notebook/Title") == "남서행 운송 기록" && FieldSession.State.hasDamagedInsulator, "journal and device");
            Check(FieldCheckpoint.CaptureInventory().Count(slot => slot.count > 0) == 28 &&
                inventory == JsonConvert.SerializeObject(FieldCheckpoint.CaptureInventory()), "full bag and base slot preserved");
            ScreenCapture.CaptureScreenshot(Path.Combine(folder, "manifest.png"));
            await Task.Delay(180);
            await Press(Key.T);
            Check(Hud("MissionBanner/StoryObjective") == "동쪽 생존자 거점 찾기", "next objective");
            await Press(Key.F);
            Check(!FieldHUD.IsOpen && !FieldSession.State.OpenMecamossCargo(), "no duplicate reward");
            await Press(Key.M);
            Check(Hud("StoryRecord/Notebook/Title") == "동부 피난 거점 약도" && GameObject.Find("EastSearchArea"), "map and search area");
            ScreenCapture.CaptureScreenshot(Path.Combine(folder, "east-map.png"));
            await Task.Delay(180);
            var book = GameObject.Find("ExplorationHUD/StoryRecord/Notebook").transform;
            book.Find("PreviousPage").GetComponent<Button>().onClick.Invoke();
            await Wait(() => book.Find("NextPage").GetComponent<Button>().interactable, "previous map");
            Check(!GameObject.Find("EastSearchArea"), "search area only on eastern sketch");
            book.Find("NextPage").GetComponent<Button>().onClick.Invoke();
            await Wait(() => book.Find("PreviousPage").GetComponent<Button>().interactable &&
                GameObject.Find("EastSearchArea"), "return to eastern sketch");
            await Press(Key.M);
            await Leave();
            FieldCheckpoint.Save(file);
            await Load(file);
            await Enter();
            Check(FieldSession.State.hasDamagedInsulator && FieldSession.State.mecamossCargoOpened, "reward persisted");
            crate = MecamossReceivingHall.Current.transform.Find("ReturnedRobotCrate");
            Managers.Game.player.GetComponent<PlayerController>().WarpTo((Vector2)crate.position + Vector2.down * .5f);
            await Wait(() => Hud("Interaction") == "빈 운송함", "empty on reentry");
            return "PASS: locked before defeat, F pickup, both objectives, journal/map, full bag, single grant, unclaimed and claimed reload";
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
