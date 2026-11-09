using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

// Real scene/serialization APIs plus targeted component callback stress, not a combat simulation.
public static class StartupLifecycleAudit
{
    static readonly List<object> results=new();
    static string prefix;
    static void Record(string label,object data)
    {
        results.Add(new{label,data});
        File.WriteAllText(prefix+"-results.json",JsonConvert.SerializeObject(results,Formatting.Indented));
    }
    static void CaptureLog(string message,string stack,LogType type)
    {
        if(type==LogType.Log && !message.Contains("STARTUP_AUDIT")) return;
        File.AppendAllText(prefix+"-logs.jsonl",JsonConvert.SerializeObject(new{type=type.ToString(),message,stack})+"\n");
    }
    static void Check(bool pass,string message)
    {if(!pass) throw new Exception(message);}
    static async Task Load(string path)
    {
        FieldCheckpoint.RequestResume(path);
        SceneManager.LoadScene("TutorialScene");
        await Task.Delay(1200);
        for(int i=0;FieldSession.ResumeRequested || !FieldInventory.Ready;i++)
        {Check(i<100,"Field load timeout");await Task.Delay(100);}
        await Task.Delay(250);
    }
    static object Buildings() => UnityEngine.Object.FindObjectsByType<Item_Buliding>()
        .OrderBy(x=>x.pos.x).ThenBy(x=>x.pos.y)
        .Select(x=>new{x=x.pos.x,y=x.pos.y,item=x.itemSo.idName,hp=x.GetComponent<Stat>().Hp}).ToArray();
    static string SavedBuildings(FieldSave save) => JsonConvert.SerializeObject(save.buildings.OrderBy(x=>x.x).ThenBy(x=>x.y));
    static void Verify(FieldSave expected,bool full)
    {
        Check(JsonConvert.SerializeObject(FieldCheckpoint.CaptureInventory())==JsonConvert.SerializeObject(expected.slots),"Inventory changed");
        Check(JsonConvert.SerializeObject(Buildings())==SavedBuildings(expected),"Facility positions or health changed");
        Check(JsonConvert.SerializeObject(FieldSession.State)==JsonConvert.SerializeObject(expected.state),"Progress changed");
        var p=Managers.Game.player.GetComponent<PlayerStat>();
        Check(p.Hp==expected.hp && p.Hunger==expected.hunger,"Player HP/hunger changed");
        Check(Managers.Game.tower.GetComponent<Stat>().Hp==expected.towerHp,"Tower HP changed");
        Check(Managers.Inven.Coin==expected.coin,"Coin changed");
        Check(UnityEngine.Object.FindObjectsByType<Managers>().Length==1,"Duplicate/missing managers");
        Check(UnityEngine.Object.FindObjectsByType<Transform>().Count(x=>x.name=="@SoundRoot")==1,"Duplicate/missing sound root");
        Check(!Managers.IsShuttingDown,"Shutdown guard did not reset");
        Check(Managers.Map != null && Managers.Map == Managers.Game.grid && Managers.Map.gameObject.scene == SceneManager.GetActiveScene(),"Map is not the active scene component");
        var root=Managers.Instance.gameObject;
        Component[] services={Managers.Game,Managers.Network,Managers.Inven,Managers.UI,Managers.Input,Managers.Scene,Managers.Data};
        Check(services.All(x=>x!=null && x.gameObject==root),"Invalid manager component ownership");
        Check(root.GetComponents<MapManager>().Length==0,"Persistent root has a stray map");
        if(full)
        {
            var current=FieldCheckpoint.Capture();
            Check(current.playerX==expected.playerX && current.playerY==expected.playerY && current.towerX==expected.towerX && current.towerY==expected.towerY,"Saved positions changed");
            Check(current.day==expected.day && current.time==expected.time,"Saved clock changed");
        }
    }
    static async Task CheckFadeReversal()
    {
        var trigger=UnityEngine.Object.FindObjectsByType<InsideBuildTrigger>().First(x=>x.roof!=null);
        var body=Managers.Game.player.GetComponent<Collider2D>();
        for(int i=0;i<8;i++)
        {
            trigger.SendMessage("OnTriggerEnter2D",body);await Task.Delay(20);
            trigger.SendMessage("OnTriggerExit2D",body);await Task.Delay(20);
        }
        await Task.Delay(1150);
        Check(Mathf.Abs(trigger.roof.color.a-1)<.001f,"Roof reversal did not finish opaque");
        trigger.SendMessage("OnTriggerEnter2D",body);await Task.Delay(1150);
        Check(trigger.roof.color.a<.001f,"Roof no longer fades on enter");
        Record("fade reversal",new{roofAlpha=trigger.roof.color.a,wallAlpha=trigger.wall!=null?trigger.wall.color.a:-1});
    }
    static void AnimateBeforeUnload(float duration=2)
    {
        var body=Managers.Game.player.GetComponent<Collider2D>();
        foreach(var t in UnityEngine.Object.FindObjectsByType<InsideBuildTrigger>()) t.SendMessage("OnTriggerExit2D",body);
        foreach(var t in UnityEngine.Object.FindObjectsByType<VisibleTile_Wall>()) t.SendMessage("OnTriggerEnter2D",body);
        Managers.Game.mainCam?.Shake(duration,.1f);
        var animator=new GameObject("LifecycleTweenOwner").AddComponent<TransformAnimator>();
        var a=new GameObject("LifecycleTweenStart").transform;
        var b=new GameObject("LifecycleTweenEnd").transform;
        a.position=b.position=Managers.Game.player.transform.position;
        var target=new GameObject("LifecycleTweenTarget").transform;
        animator.MoveTransformToTarget(target,a,b,duration);
    }
    public static object PrepareStop(string label)
    {
        AnimateBeforeUnload(30);
        Managers.Inven.inventoryUI.gameObject.SetActive(true);
        var state=new{managers=UnityEngine.Object.FindObjectsByType<Managers>().Length};
        File.WriteAllText("Logs/StartupWarnings/"+label+"-before-stop.json",JsonConvert.SerializeObject(state));
        return state;
    }
    public static object VerifyStopped(string label)
    {
        Check(!UnityEditor.EditorApplication.isPlaying,"Still playing");
        // Even an accidental late acquisition must not create an Editor object.
        Check(Managers.Instance==null,"Singleton survived shutdown");
        int managerCount=UnityEngine.Object.FindObjectsByType<Managers>(FindObjectsInactive.Include).Length;
        int soundRoots=UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include).Count(x=>x.name=="@SoundRoot");
        Check(managerCount==0 && soundRoots==0,"Objects recreated at shutdown");
        var state=new{managerCount,soundRoots};
        File.WriteAllText("Logs/StartupWarnings/"+label+"-after-stop.json",JsonConvert.SerializeObject(state));
        return state;
    }
    static async Task EnterFactory()
    {
        var door=UnityEngine.Object.FindAnyObjectByType<Boss1EnterTrigger>();
        var button=door.GetComponentsInChildren<Transform>(true).First(x=>x.name=="Enter").GetComponent<UI_EventHandler>();
        button.OnPointerClick(new PointerEventData(EventSystem.current));
        for(int i=0;SceneManager.GetActiveScene().name!="Boss1";i++)
        {Check(i<100,"Factory transition timeout");await Task.Delay(100);}
        await Task.Delay(500);
        for(int i=0;FieldCheckpoint.TransferPreparation!=null || !FieldInventory.Ready;i++)
        {Check(i<100,"Transfer restore timeout");await Task.Delay(100);}
    }
    public static async Task<object> Run(string label,int cycles)
    {
        Directory.CreateDirectory("Logs/StartupWarnings");prefix="Logs/StartupWarnings/"+label;
        File.WriteAllText(prefix+"-logs.jsonl","");
        Application.logMessageReceived+=CaptureLog;
        var source="docs/level-design/v03-postnight-final-pre-factory.json";
        var expected=JsonConvert.DeserializeObject<FieldSave>(File.ReadAllText(source));
        try
        {
            await Load(source);Verify(expected,true);await CheckFadeReversal();
            for(int i=0;i<cycles;i++)
            {
                var save=prefix+"-save.json";
                FieldCheckpoint.Save(save);
                AnimateBeforeUnload();
                await Load(save);Verify(expected,true);
                await EnterFactory();Verify(expected,false);
                // Walk into the real intro trigger, then reload during its camera/dialog sequence.
                Managers.Game.player.SendMessage("SetMoveDirection",Vector2.up);
                await Task.Delay(2100);
                Managers.Game.player.SendMessage("SetMoveDirection",Vector2.zero);
                AnimateBeforeUnload();
                bool cutsceneLocked=Managers.Game.isCantPlay;
                await Load(save);Verify(expected,true);
                await Task.Delay(1200);
                Record("cycle "+i,new{cutsceneLocked,managers=UnityEngine.Object.FindObjectsByType<Managers>().Length,
                    inventory=FieldCheckpoint.CaptureInventory(),buildings=Buildings()});
            }
            AnimateBeforeUnload();
            Managers.Inven.inventoryUI.gameObject.SetActive(true);
            Record("ready for Play Mode stop",new{cycles});
            return new{success=true,cycles,results};
        }
        catch(Exception e){Record("FAILED",e.ToString());throw;}
        // Keep the log subscription alive through Play Mode shutdown to catch cleanup errors.
    }
}
