using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
public static class ExplorationPersistenceSmoke
{
    static readonly List<string> checks=new List<string>();
    static void Check(bool ok,string label) { if(!ok) throw new Exception("FAIL: "+label); checks.Add(label); Debug.Log("EXPLORATION_TEST: "+label); }
    static async Task Delay(float seconds) { float end=Time.time+seconds; while(Time.time<end) await Task.Delay(20); }
    public static async Task<string> Run()
    {
        checks.Clear(); var c=ExplorationRuntime.Instance.content;
        Check(Application.isPlaying && FieldInventory.Ready,"Ready for persistence tests");
        FieldSession.State=new ExplorationState { firstNightCleared=true };
        foreach(var slot in FieldInventory.Slots) slot.MakeEmptySlot();
        Managers.Game.isCantPlay=false; PlayerController.isDie=false;
        Managers.Game.tower.transform.position=new Vector2(70,-70); Managers.Game.player.transform.position=new Vector2(58,-69);
        var cannon=Resources.Load<ItemSO>("Item/Cannon"); var wall=Resources.Load<ItemSO>("Item/Fence");
        MapManager.building.SetTile(new Vector3Int(1,0,0),cannon.buildTile);
        MapManager.building.SetTile(new Vector3Int(2,0,0),wall.buildTile);
        await Delay(.3f);
        var gun=MapManager.building.GetInstantiatedObject(new Vector3Int(1,0,0)).GetComponent<BuildStat>();
        var fence=MapManager.building.GetInstantiatedObject(new Vector3Int(2,0,0)).GetComponent<Stat>();
        Check(gun.maxHP==20 && fence.maxHP==30,"Base cannon and wall health");
        gun.Hp=12; fence.Hp=17;
        FieldSession.State.research.Add("U2"); FieldSession.State.research.Add("U3"); ExplorationEffects.ApplyAll();
        Check(gun.Hp==12 && gun.maxHP==30 && fence.Hp==17 && fence.maxHP==45 && gun.Damage==7,"Existing buildings upgraded without healing");
        MapManager.building.SetTile(new Vector3Int(3,0,0),cannon.buildTile); await Delay(.2f);
        var fresh=MapManager.building.GetInstantiatedObject(new Vector3Int(3,0,0)).GetComponent<BuildStat>();
        Check(fresh.Hp==30 && fresh.maxHP==30 && fresh.Damage==7,"New cannon starts with research stats");
        MapManager.building.SetTile(new Vector3Int(4,0,0),cannon.buildTile); await Delay(.2f);
        Check(ExplorationEffects.CannonCount()==3,"RuleTile cannon slots counted including attached facilities");
        Managers.Game.tower.SetBeforeBuild();
        Check(ExplorationEffects.CannonCount()==3,"Transport retains reserved cannon slots");
        Managers.Game.tower.SetAfterBuild();
        FieldInventory.Give(c.scrap,7); FieldInventory.Give(c.parts,2);
        FieldSession.State.AddClaim("A1:0",3); FieldSession.State.killedGuards.Add("D1a");
        var tower=Managers.Game.tower.GetComponent<Stat>(); tower.Hp=321;
        string path=System.IO.Path.GetFullPath("Logs/exploration-test-save.json");
        FieldCheckpoint.Save(path);
        var snapshot=FieldCheckpoint.Read(c,path);
        Check(snapshot.buildings.Count==4 && snapshot.state.Has("U3"),"Disk snapshot includes buildings and research");
        FieldCheckpoint.Save(path);
        Check(System.IO.File.Exists(path+".bak"),"Atomic replacement preserves previous save");
        foreach(var slot in FieldInventory.Slots) slot.MakeEmptySlot();
        FieldSession.State=new ExplorationState(); gun.Hp=1; tower.Hp=1;
        var restore=FieldCheckpoint.Restore(c,path);
        while(restore.MoveNext()) await Delay(.1f);
        await Delay(.2f);
        gun=MapManager.building.GetInstantiatedObject(new Vector3Int(1,0,0)).GetComponent<BuildStat>();
        Check(FieldInventory.Count(c.scrap)==7 && FieldInventory.Count(c.parts)==2 && FieldSession.State.Claimed("A1:0")==3 && FieldSession.State.killedGuards.Contains("D1a"),"Inventory, depletion and kills restored together");
        Check(gun.Hp==12 && gun.maxHP==30 && tower.Hp==321 && FieldSession.State.Has("U3"),"Building health and research restored without healing");
        FieldCheckpoint.Transfer=FieldCheckpoint.CaptureInventory();
        foreach(var slot in FieldInventory.Slots) slot.MakeEmptySlot();
        var transfer=FieldCheckpoint.RestoreTransfer(); while(transfer.MoveNext()) await Delay(.1f);
        Check(FieldInventory.Count(c.parts)==2 && FieldInventory.Count(c.scrap)==7,"Boss transition inventory transfer");
        // Leave a reviewable research panel. This is a camera-rendered test fixture, not a desktop screenshot.
        var station=UnityEngine.Object.FindAnyObjectByType<FieldWorkbench>();
        Managers.Game.player.transform.position=station.transform.position;
        FieldHUD.Open(station); await Delay(.2f);
        var canvas=UnityEngine.Object.FindAnyObjectByType<FieldHUD>().GetComponent<Canvas>();
        canvas.renderMode=RenderMode.ScreenSpaceCamera; canvas.worldCamera=Camera.main; canvas.planeDistance=1;
        System.IO.File.WriteAllText("Logs/exploration-persistence-smoke.json",Newtonsoft.Json.JsonConvert.SerializeObject(checks,Newtonsoft.Json.Formatting.Indented));
        return string.Join("\n",checks);
    }
}
