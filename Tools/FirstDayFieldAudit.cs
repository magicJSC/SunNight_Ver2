using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// C1 fixture only. After Setup, use walking, held interaction, crafting UI and normal placement.
// No time acceleration, teleport, resource grant, damage injection or enemy deletion during a trial.
public static class FirstDayFieldAudit
{
    static float start, distance, minHp, lastHp, chaseSeconds;
    static Vector2 previous;
    static int hits, maxNearby;
    static string routeName, phase;
    static readonly HashSet<string> encounters = new HashSet<string>();
    static readonly List<object> steps = new List<object>();
    static bool trackingNight;
    static float nightStarted, minHunger, minTowerHp, lastProgress;
    static int foodUsed, peakWaveAlive;
    static object preparation, nightBeginning;
    static float nightEnded, nightClockMin, nightClockMax;
    static object postClearCheck;
    static readonly List<object> nightEvents=new List<object>();
    static readonly HashSet<string> waveIds=new HashSet<string>();
    static readonly HashSet<string> deadWaveIds=new HashSet<string>();
    sealed class FacilityReading { public string item,id; public float hp,maxHp; [JsonIgnore] public Stat stat; }
    static readonly List<FacilityReading> facilities=new List<FacilityReading>();
    static readonly System.Reflection.FieldInfo waveListField=typeof(MonsterWaveController).GetField("monList",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
    static PlayerController Player => Managers.Game.player;
    static PlayerStat Stat => Player.GetComponent<PlayerStat>();
    static ItemSO Item(string id) => Resources.Load<ItemSO>("Item/"+id);
    static int Count(string id) => FieldInventory.Count(Item(id));
    static object Snapshot() => new { route=routeName,phase,seconds=Time.time-start,clock=Managers.Game.timeController.TimeAmount,
        position=new[]{Player.transform.position.x,Player.transform.position.y},distance,hp=Stat.Hp,minHp,hits,hunger=Stat.Hunger,
        scrap=Count("IronOre"),food=Count("Steak"),encounters=encounters.OrderBy(x=>x).ToArray(),chaseSeconds,maxNearby,
        cannons=ExplorationEffects.CannonCount(),walls=UnityEngine.Object.FindObjectsByType<Item_Buliding>().Count(x=>x.itemSo.idName=="Fence"),
        towerHp=Managers.Game.tower.GetComponent<Stat>().Hp,day=Managers.Game.timeController.day,foodUsed,minHunger,minTowerHp,
        nightSeconds=nightStarted<0?0:(nightEnded>=0?nightEnded:Time.time)-nightStarted,
        waveSpawned=waveIds.Count,waveKilled=deadWaveIds.Count,peakWaveAlive,firstNightCleared=FieldSession.State.firstNightCleared};
    static void SaveProgress() => System.IO.File.WriteAllText("Logs/FirstDayV03/"+routeName+"-progress.json",JsonConvert.SerializeObject(Snapshot(),Formatting.Indented));
    static async Task Wait(float seconds)
    {
        float until=Time.time+seconds, last=Time.time;
        while(Time.time<until)
        {
            await Task.Delay(30);
            Vector2 current=Player.transform.position; distance+=Vector2.Distance(previous,current); previous=current;
            minHp=Mathf.Min(minHp,Stat.Hp); if(Stat.Hp<lastHp) hits++; lastHp=Stat.Hp;
            minHunger=Mathf.Min(minHunger,Stat.Hunger); minTowerHp=Mathf.Min(minTowerHp,Managers.Game.tower.GetComponent<Stat>().Hp);
            if(trackingNight) ObserveNight();
            var monsters=UnityEngine.Object.FindObjectsByType<FieldGuard>();
            int nearby=monsters.Count(x=>Vector2.Distance(current,x.transform.position)<10); maxNearby=Mathf.Max(maxNearby,nearby);
            bool chased=false;
            foreach(var m in monsters) if(m.GetComponent<MonsterController>().target==Player.transform)
            { encounters.Add(m.guardId); chased=true; }
            if(chased) chaseSeconds+=Time.time-last;
            last=Time.time;
            if(PlayerController.isDie) throw new Exception("Player died");
            if(Time.time-start>(routeName.Contains("night")?900:470)) throw new Exception("Trial time budget exceeded");
        }
    }
    static async Task Walk(Vector2 target)
    {
        float begin=Time.time,lastMove=Time.time; Vector2 check=Player.transform.position;
        try
        {
            while(Vector2.Distance(Player.transform.position,target)>.3f)
            {
                Vector2 pos=Player.transform.position;
                if(Vector2.Distance(pos,check)>.15f) {check=pos;lastMove=Time.time;}
                if(Time.time-lastMove>3 || Time.time-begin>100) throw new Exception("Walk blocked toward "+target+" at "+pos);
                Player.SendMessage("SetMoveDirection",(target-pos).normalized);
                await Wait(.03f);
            }
        }
        finally { Player.SendMessage("SetMoveDirection",Vector2.zero); }
    }
    static async Task Gather(string id)
    {
        phase="walk "+id; SaveProgress();
        var data=ExplorationRuntime.Instance.content.rewards.First(x=>x.id==id);
        await Walk(data.position);
        await Wait(3); // Explicit decision/inspection allowance at every pickup, not claimed as human input.
        var reward=GameObject.Find("Reward_"+id).GetComponent<FieldReward>();
        var interact=Player.GetComponentInChildren<PlayerInteract>();
        if(!interact.interactObjectList.Contains(reward.gameObject)) throw new Exception("Out of interaction range "+id);
        phase="collect "+id;
        interact.SetInteractObj(); interact.interactAction?.Invoke();
        await Wait(data.collectSeconds+.25f); interact.cancelAction?.Invoke();
        if(!reward.Empty) throw new Exception("Collection interrupted "+id);
        steps.Add(Snapshot()); SaveProgress();
    }
    static async Task Setup()
    {
        for(int n=0;!FieldInventory.Ready || UnityEngine.Object.FindObjectsByType<FieldReward>().Length!=45;n++)
        {if(n>200) throw new Exception("Scene not ready"); await Task.Delay(100);}
        FieldSession.NewRun(); FieldSession.State.layoutVersion=3; FieldSession.Active=true;
        PlayerController.isDie=false; Managers.Game.isCantPlay=false; Managers.Game.isHandleUI=false; Managers.Game.isMovingTower=false;
        Managers.Game.timeController.gameObject.SetActive(false);
        Player.transform.position=new Vector2(-33,-42); Player.SendMessage("SetMoveDirection",Vector2.zero);
        Managers.Game.tower.transform.position=new Vector2(-33,-19);
        Stat.Hp=100; Stat.Hunger=30; ExplorationEffects.ApplyAll(); Managers.Game.tower.GetComponent<Stat>().Hp=500;
        foreach(var slot in FieldInventory.Slots) {slot.MakeEmptySlot(); slot.SetEmptyItem();}
        FieldInventory.Give(Item("Bat"),1); FieldInventory.Give(Item("Steak"),1);
        MapManager.building.ClearAllTiles(); MapManager.buildData.Clear();
        MapManager.building.SetTile(new Vector3Int(-1,1,0),Item("Cannon").buildTile);
        foreach(var cell in new[]{new Vector3Int(-1,2,0),new Vector3Int(0,2,0)}) MapManager.building.SetTile(cell,Item("Fence").buildTile);
        var defense=UnityEngine.Object.FindAnyObjectByType<DefenseEvent>();
        defense.close.SetActive(false); defense.open.SetActive(true); defense.chain.SetActive(false);
        Managers.Game.canMoveTower=true;
        await Task.Delay(600);
        Managers.Map.SetCanBuildTile();
        Managers.Game.timeController.RestoreClock(1,330); Managers.Game.timeController.gameObject.SetActive(true);
        start=Time.time;previous=Player.transform.position;minHp=lastHp=Stat.Hp;distance=chaseSeconds=0;hits=maxNearby=0;
        trackingNight=false;nightStarted=-1;minHunger=30;minTowerHp=500;foodUsed=peakWaveAlive=0;lastProgress=0;
        nightEnded=-1;nightClockMin=1440;nightClockMax=0;postClearCheck=null;
        preparation=nightBeginning=null;nightEvents.Clear();waveIds.Clear();deadWaveIds.Clear();facilities.Clear();
        encounters.Clear(); steps.Clear(); phase="C1 fixture";steps.Add(Snapshot());
    }
    static async Task CraftAndBuild()
    {
        phase="return"; SaveProgress();
        await Walk(new Vector2(-33,-42)); await Walk(new Vector2(-33,-34)); await Walk(new Vector2(-33,-23));
        steps.Add(Snapshot());
        phase="craft";
        Player.SendMessage("OnProduceUI"); await Wait(4);
        var ui=UnityEngine.Object.FindAnyObjectByType<UI_Produce>();
        if(ui==null || !ui.gameObject.activeInHierarchy) throw new Exception("Crafting UI did not open");
        foreach(string id in new[]{"Cannon","Cannon","Fence","Fence","Fence","Fence"})
        {
            var recipe=ui.GetComponentsInChildren<UI_Produce_Item>().First(x=>x.toMakeItem.toMakeItemSO.idName==id);
            recipe.SetProduce(); await Wait(2);
            int before=Count(id); ui.SendMessage("OnProduce"); await Wait(.2f);
            if(Count(id)!=before+1) throw new Exception("Craft failed "+id);
        }
        ui.gameObject.SetActive(false); Managers.Inven.inventoryUI.gameObject.SetActive(false); await Wait(.5f);
        steps.Add(Snapshot()); phase="build";
        var builder=UnityEngine.Object.FindAnyObjectByType<BuildController>(FindObjectsInactive.Include);
        var cells=new[]{new Vector3Int(1,1,0),new Vector3Int(0,1,0),new Vector3Int(1,2,0),new Vector3Int(2,-1,0),new Vector3Int(2,0,0),new Vector3Int(2,1,0)};
        for(int i=0;i<cells.Length;i++)
        {
            string id=i<2?"Cannon":"Fence";
            var slot=FieldInventory.Slots.First(x=>x.slotInfo.itemInfo==Item(id) && x.slotInfo.count>0);
            int hotIndex=Array.FindIndex(Managers.Inven.hotBarUI.slotList,x=>x.itemUI==slot);
            if(hotIndex<0)
            {
                hotIndex=4; Managers.Inven.ChangeItem(slot,Managers.Inven.hotBarUI.slotList[hotIndex].itemUI);
            }
            Managers.Inven.hotBarUI.SendMessage("ChangeChoice",hotIndex);
            Managers.Inven.CheckHotBarChoice(); Managers.Map.SetCanBuildTile();
            await Wait(5);
            if(!Managers.Map.CheckCanUseTile(cells[i])) throw new Exception("Build cell blocked "+cells[i]);
            int before=Count(id);
            builder.BuildAtWorldPoint(MapManager.building.GetCellCenterWorld(cells[i])); await Wait(.2f);
            if(Count(id)!=before-1 || !MapManager.building.HasTile(cells[i])) throw new Exception("Build failed "+cells[i]+" UI="+Managers.Game.isHandleUI);
        }
        if(ExplorationEffects.CannonCount()!=3) throw new Exception("Expected 3 cannons");
        steps.Add(Snapshot());
    }
    static async Task SearchVisibleSupplies()
    {
        // Observe only markers inside the actual gameplay camera, then remember discovered sites.
        // These broad survey points are not pickup coordinates and do not paint paths into the map.
        var seen=new HashSet<string>();
        var survey=new[]{new Vector2(-40,-65),new Vector2(65,-55),new Vector2(65,-80),new Vector2(-10,-80),
            new Vector2(-10,-105),new Vector2(65,-105),new Vector2(65,-130),new Vector2(0,-130),
            new Vector2(0,-155),new Vector2(65,-155),new Vector2(65,-180),new Vector2(0,-180),
            new Vector2(65,-195),new Vector2(65,-20),new Vector2(-10,-20)};
        foreach(var point in survey)
        {
            float legStart=Time.time;
            while(Vector2.Distance(Player.transform.position,point)>.4f)
            {
                foreach(var reward in ExplorationRuntime.Instance.content.rewards.Where(x=>x.group=="" && !x.requiresFirstNight))
                {
                    var view=Camera.main.WorldToViewportPoint((Vector3)reward.position+Vector3.up*1.3f);
                    if(view.z>0 && view.x>.02f && view.x<.98f && view.y>.02f && view.y<.98f) seen.Add(reward.id);
                }
                var next=ExplorationRuntime.Instance.content.rewards.Where(x=>seen.Contains(x.id) && x.items[0]=="IronOre" && FieldSession.State.Claimed(x.id+":0")==0)
                    .OrderBy(x=>Vector2.Distance(Player.transform.position,x.position)).FirstOrDefault();
                if(next!=null)
                {
                    Player.SendMessage("SetMoveDirection",Vector2.zero);
                    await Gather(next.id);
                    if(Count("IronOre")>=14) return;
                    legStart=Time.time;
                }
                else
                {
                    phase="visible search toward "+point;
                    Player.SendMessage("SetMoveDirection",(point-(Vector2)Player.transform.position).normalized);
                    await Wait(.05f);
                    if(Time.time-legStart>75) throw new Exception("Survey leg blocked "+point);
                }
            }
            Player.SendMessage("SetMoveDirection",Vector2.zero); SaveProgress();
        }
        throw new Exception("Survey finished without 14 visible scrap");
    }
    static void ObserveNight()
    {
        if(nightStarted<0 && Managers.Game.timeController.timeType==TimeController.TimeType.Night)
        { nightStarted=Time.time;phase="first night";nightBeginning=Snapshot();nightEvents.Add(new{kind="night began",state=nightBeginning}); }
        if(nightStarted>=0 && !FieldSession.State.firstNightCleared)
        {
            var clock=Managers.Game.timeController;
            nightClockMin=Mathf.Min(nightClockMin,clock.TimeAmount);nightClockMax=Mathf.Max(nightClockMax,clock.TimeAmount);
            if(routeName.Contains("clockfix") && (clock.day!=1 || Mathf.Abs(clock.TimeAmount-1290)>.01f))
                throw new Exception("First-night clock moved before combat clear");
        }
        var wave=UnityEngine.Object.FindAnyObjectByType<MonsterWaveController>();
        var alive=((List<GameObject>)waveListField.GetValue(wave)).Where(x=>x!=null).ToArray();
        peakWaveAlive=Mathf.Max(peakWaveAlive,alive.Length);
        foreach(var go in alive)
        {
            string id=go.GetEntityId().ToString(); if(!waveIds.Add(id)) continue;
            var stat=go.GetComponent<MonsterStat>();
            nightEvents.Add(new{kind="spawn",seconds=Time.time-start,id,name=go.name,hp=stat.Hp,position=go.transform.position.ToString()});
            go.GetComponent<MonsterController>().dieEvent+=ignored=>{deadWaveIds.Add(id);nightEvents.Add(new{kind="kill",seconds=Time.time-start,id});};
        }
        foreach(var f in facilities) f.hp=f.stat==null?0:f.stat.Hp;
        if(Time.time-lastProgress>2) {lastProgress=Time.time;SaveProgress();}
    }
    static async Task FinishNight()
    {
        if(preparation==null) preparation=Snapshot();
        System.IO.File.WriteAllText("Logs/FirstNightV03/"+routeName+"-earned-preparation.json",JsonConvert.SerializeObject(FieldCheckpoint.Capture(),Formatting.Indented));
        foreach(var b in UnityEngine.Object.FindObjectsByType<Item_Buliding>().Where(x=>x.transform.IsChildOf(MapManager.building.transform)))
        {var s=b.GetComponent<Stat>();facilities.Add(new FacilityReading{item=b.itemSo.idName,id=b.GetEntityId().ToString(),hp=s.Hp,maxHp=s.maxHP,stat=s});}
        trackingNight=true;phase="waiting for natural night";
        var previousBackground=InputSystem.settings.backgroundBehavior;
        var previousEditorInput=InputSystem.settings.editorInputBehaviorInPlayMode;
        InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
        InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        var keyboard=InputSystem.AddDevice<Keyboard>("FirstNightAuditKeyboard");
        var eat=Player.GetComponentInChildren<EatController>();
        using(var action=new InputAction("FirstNightAuditEat",InputActionType.Button,"<Keyboard>/f13"))
        {
            action.performed+=eat.EatActoin; action.Enable();
            try
            {
                while(!FieldSession.State.firstNightCleared)
                {
                    if(Managers.Game.tower.GetComponent<Stat>().Hp<=0) throw new Exception("Tower destroyed");
                    var near=UnityEngine.Object.FindObjectsByType<MonsterController>()
                        .OrderBy(x=>Vector2.Distance(x.transform.position,Player.transform.position)).FirstOrDefault();
                    Vector2 anchor=(Vector2)Managers.Game.tower.transform.position+Vector2.down*4;
                    Vector2 pos=Player.transform.position, direction=Vector2.zero;
                    if(near!=null && Vector2.Distance(near.transform.position,pos)<3)
                        direction=(pos-(Vector2)near.transform.position).normalized;
                    else if(Vector2.Distance(anchor,pos)>.6f) direction=(anchor-pos).normalized;
                    Player.SendMessage("SetMoveDirection",direction);
                    if(Stat.Hunger<=8 && Count("Steak")>0)
                    {
                        Player.SendMessage("SetMoveDirection",Vector2.zero);
                        int hot=Array.FindIndex(Managers.Inven.hotBarUI.slotList,x=>x.itemUI.slotInfo.itemInfo==Item("Steak"));
                        if(hot<0) throw new Exception("Food missing from hotbar");
                        Managers.Inven.hotBarUI.SendMessage("ChangeChoice",hot); Managers.Inven.CheckHotBarChoice();
                        int before=Count("Steak"); float hungerBefore=Stat.Hunger;
                        InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.F13)); await Wait(.15f);
                        InputSystem.QueueStateEvent(keyboard,new KeyboardState()); await Wait(.15f);
                        if(Count("Steak")!=before-1 || Stat.Hunger<hungerBefore+11) throw new Exception("Food input did not consume and restore hunger");
                        foodUsed++;nightEvents.Add(new{kind="ate food",seconds=Time.time-start,hungerBefore,hungerAfter=Stat.Hunger,foodLeft=Count("Steak")});
                    }
                    await Wait(.08f);
                }
                ObserveNight();
                if(waveIds.Count!=15 || deadWaveIds.Count!=15) throw new Exception("Unexpected first-night enemy ledger");
                nightEnded=Time.time;
                phase="first night cleared"; steps.Add(Snapshot());
                if(routeName.Contains("clockfix"))
                {
                    float hunger=Stat.Hunger,hp=Stat.Hp,towerHp=Managers.Game.tower.GetComponent<Stat>().Hp;
                    int scrap=Count("IronOre");
                    float clearedAt=Managers.Game.timeController.TimeAmount;
                    await Wait(5);
                    bool stable=Managers.Game.timeController.day==2 && Managers.Game.timeController.TimeAmount>clearedAt &&
                        Mathf.Abs(Stat.Hunger-hunger)<.01f && Stat.Hp==hp && Managers.Game.tower.GetComponent<Stat>().Hp==towerHp &&
                        Count("IronOre")==scrap && waveIds.Count==15;
                    postClearCheck=new{secondsObserved=5,stable,hunger,hp,towerHp,scrap};
                    if(!stable) throw new Exception("Day 2 clock failed to advance or defense rewards/stats changed");
                }
            }
            finally {action.Disable();InputSystem.RemoveDevice(keyboard);InputSystem.settings.backgroundBehavior=previousBackground;InputSystem.settings.editorInputBehaviorInPlayMode=previousEditorInput;Player.SendMessage("SetMoveDirection",Vector2.zero);}
        }
    }
    static void ResumeEarnedPreparation()
    {
        // Continue the still-live run after the input harness stopped. No inventory/HP/claims/tiles restored or granted.
        string source=System.IO.File.Exists("Logs/FirstDayV03/search-night-resume.json") ? "Logs/FirstDayV03/search-night-resume.json" : "Logs/FirstDayV03/search-night-before.json";
        var data=Newtonsoft.Json.Linq.JObject.Parse(System.IO.File.ReadAllText(source));
        var s=data["summary"];
        if(Count("IronOre")!=0 || Count("Steak")!=1 || ExplorationEffects.CannonCount()!=3 ||
            Mathf.Abs(Stat.Hunger-(float)s["hunger"])>.01f || Stat.Hp!=(float)s["hp"] || FieldSession.State.firstNightCleared)
            throw new Exception("Earned preparation changed; refuse to resume a different run");
        start=Time.time-(float)s["seconds"];previous=Player.transform.position;distance=(float)s["distance"];
        minHp=(float)s["minHp"];lastHp=Stat.Hp;hits=(int)s["hits"];minHunger=(float)s["minHunger"];minTowerHp=(float)s["minTowerHp"];
        chaseSeconds=(float)s["chaseSeconds"];maxNearby=(int)s["maxNearby"];foodUsed=peakWaveAlive=0;nightStarted=-1;
        nightEnded=-1;nightClockMin=1440;nightClockMax=0;postClearCheck=null;
        steps.Clear();foreach(var step in data["steps"]) steps.Add(step);
        encounters.Clear();foreach(var id in s["encounters"]) encounters.Add((string)id);
        facilities.Clear();waveIds.Clear();deadWaveIds.Clear();nightEvents.Clear();
        preparation=data["night"]["preparation"];nightBeginning=null;
        nightEvents.Add(new{kind="resume after input harness pause",source,activeSeconds=(float)s["seconds"],
            wallPauseSeconds=(DateTime.UtcNow-System.IO.File.GetLastWriteTimeUtc(source)).TotalSeconds,
            reason="Headless keyboard disabled by background input setting. World and hunger clock paused; all earned state retained."});
        var clock=Managers.Game.timeController;clock.gameObject.SetActive(true);clock.RestoreClock(1,(float)s["clock"]);
    }
    public static async Task<object> Run(string route)
    {
        routeName=route; string error=null;
        try
        {
            if(route.Contains("resume")) {ResumeEarnedPreparation();await FinishNight();}
            else
            {
            await Setup();
            string[] ids;
            if(route.StartsWith("south")) ids="A1_s1 A1 A1_s2 A3 A3_s1 A3_s2 A4 A2 A2_s1 A2_s2 B1 B1_s1 B2 B4 B2_s1 B3_s1".Split(' ');
            else if(route.StartsWith("west")) ids="A1_s1 C1_s1 C1 C1_s2 C3 C2_s1 C2_s2 C2 A2_s2 A2_s1 A2 A4 A3 A3_s1 A1 A1_s2".Split(' ');
            else ids="A1_s1 A1 A1_s2 A3 A3_s1 A3_s2 E0 E0_s1 E0_s2 E0_s3 D0 A2_s1 A2_s2 A2 A4".Split(' ');
            if(route.StartsWith("search")) await SearchVisibleSupplies();
            else foreach(var id in ids) await Gather(id);
            await CraftAndBuild(); phase="ready";
            if(route.Contains("night")) await FinishNight();
            }
        }
        catch(Exception e) {error=e.ToString();phase="failed: "+e.Message;}
        finally
        {
            if(Player!=null) Player.SendMessage("SetMoveDirection",Vector2.zero);
            var result=new{fixture="C1: Bat1, Steak1, cannon1, walls2, HP100 hunger30 at 05:30. No grants or teleport after fixture.",
                policy=(route.StartsWith("search") ? "Survey with only camera-visible resource markers discovered and remembered; " : "Known-coordinate route; ")+
                    "base-speed walking, actual held collection, UI crafting and hotbar-selected placement. 3s inspection per node, 2s recipe selection, 5s placement included. No attacks or sprint.",
                error,success=error==null,summary=Snapshot(),steps,
                night=route.Contains("night")?new{preparation,nightBeginning,facilities,events=nightEvents,nightClockMin,nightClockMax,postClearCheck,
                    policy="Natural clock, no acceleration, no player attacks, walk away from enemies within 3, return near tower, actual F13 input to EatActoin at hunger <=8. No healing, repairs or extra building."}:null};
            System.IO.File.WriteAllText("Logs/FirstDayV03/"+routeName+".json",JsonConvert.SerializeObject(result,Formatting.Indented));
            Debug.Log("FIRST_DAY_AUDIT "+JsonConvert.SerializeObject(result.summary));
            Managers.Game.timeController.gameObject.SetActive(false);
        }
        return new{route,error,summary=Snapshot()};
    }

    public static async Task<object> ReplayEarnedNight()
    {
        routeName="postnight-replay-clockfix";
        await Setup();
        var data=JsonConvert.DeserializeObject<FieldSave>(System.IO.File.ReadAllText("docs/level-design/v03-firstnight-earned-preparation.json"));
        Managers.Game.timeController.gameObject.SetActive(false);
        FieldCheckpoint.TransferPreparation=data;
        bool restored=false;
        System.Collections.IEnumerator Restore()
        { yield return FieldCheckpoint.RestoreTransfer(); restored=true; }
        Player.StartCoroutine(Restore());
        while(!restored) await Task.Delay(30);
        Player.transform.position=new Vector2(data.playerX,data.playerY);
        Managers.Game.tower.transform.position=new Vector2(data.towerX,data.towerY);
        previous=Player.transform.position;start=Time.time;minHunger=data.hunger;
        Managers.Game.timeController.gameObject.SetActive(true);
        Managers.Game.timeController.RestoreClock(data.day,data.time);
        await FinishNight();
        var checkpoint=FieldCheckpoint.Capture();
        System.IO.File.WriteAllText("Logs/PostNightV03/earned-postnight.json",JsonConvert.SerializeObject(checkpoint,Formatting.Indented));
        var result=new{policy="Replay from the exact saved earned pre-night checkpoint, natural clock and actual combat. No resources or damage injected after restoration.",summary=Snapshot(),facilities,events=nightEvents};
        System.IO.File.WriteAllText("Logs/PostNightV03/night-replay.json",JsonConvert.SerializeObject(result,Formatting.Indented));
        return result.summary;
    }
}
