using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

// Run in an isolated Play Mode session through `unity command script run`.
// This intentionally mutates only the current Play Mode test state.
public static class ExplorationSmoke
{
    static readonly List<string> checks=new List<string>();
    static void Check(bool ok,string label) { if(!ok) throw new Exception("FAIL: "+label); checks.Add(label); Debug.Log("EXPLORATION_TEST: "+label); }
    static void Empty() { foreach(var slot in FieldInventory.Slots) slot.MakeEmptySlot(); }
    static async Task Delay(float seconds) { float until=Time.time+seconds; while(Time.time<until) await Task.Delay(20); }
    public static async Task<string> Run()
    {
        checks.Clear();
        Check(Application.isPlaying && FieldInventory.Ready,"Play Mode initialized");
        int frame=Time.frameCount; await Task.Delay(100); Check(Time.frameCount>frame,"Player loop advances");
        var c=ExplorationRuntime.Instance.content;
        Check(UnityEngine.Object.FindObjectsByType<FieldReward>().Length==c.rewards.Length,"Configured reward containers spawned");
        Check(UnityEngine.Object.FindObjectsByType<FieldGuard>().Length==c.guards.Length,"Configured regional guards and patrols spawned");
        foreach(var director in UnityEngine.Object.FindObjectsByType<UnityEngine.Playables.PlayableDirector>()) director.Stop();
        Managers.Game.isCantPlay=false; PlayerController.isDie=false;
        var player=Managers.Game.player; var interact=player.GetComponentInChildren<PlayerInteract>();
        Check(interact!=null,"Existing player interaction component");
        Empty(); FieldSession.State=new ExplorationState { layoutVersion=3 };
        foreach(var guard in c.guards.Where(g=>g.group=="F")) FieldSession.State.killedGuards.Add(guard.id); // Loot fixture: bypass F guard lock only.
        var a=GameObject.Find("Reward_F1").GetComponent<FieldReward>();
        player.transform.position=a.transform.position; await Delay(.15f);
        Check(interact.interactObjectList.Contains(a.gameObject),"Physics trigger registers reward with player");
        interact.SetInteractObj(); interact.interactAction?.Invoke(); await Delay(.2f); interact.cancelAction?.Invoke();
        Check(FieldInventory.Count(c.scrap)==0 && FieldSession.State.Claimed("F1:0")==0,"Cancelled collection grants nothing");
        foreach(var slot in FieldInventory.Slots) Managers.Inven.SetSlot(c.food,slot,c.food.maxAmount);
        Managers.Inven.SetSlot(c.scrap,FieldInventory.Slots.First(),63);
        a.ShowInteractSign(); a.SendMessage("Interact"); await Delay(4.3f);
        Check(FieldInventory.Count(c.scrap)==64 && FieldSession.State.Claimed("F1:0")==1 && !a.Empty,"Full bag accepts only one and keeps two in chest");
        FieldInventory.Slots.Last().MakeEmptySlot(); a.SendMessage("Interact"); await Delay(4.3f);
        Check(FieldSession.State.Claimed("F1:0")==3 && a.Empty,"Remaining reward can be collected");
        int before=FieldInventory.Count(c.scrap); a.SendMessage("Interact"); await Delay(.2f);
        Check(FieldInventory.Count(c.scrap)==before,"Depleted chest cannot duplicate rewards");
        var d=GameObject.Find("Reward_D1").GetComponent<FieldReward>(); player.transform.position=d.transform.position;
        await Delay(.15f); d.ShowInteractSign(); d.SendMessage("Interact"); await Delay(.2f);
        Check(FieldSession.State.Claimed("D1:0")==0,"Combat reward locked before guards die");
        foreach(var guard in UnityEngine.Object.FindObjectsByType<FieldGuard>().Where(g=>g.guardId.StartsWith("D1"))) guard.GetComponent<MonsterController>().GetDamage(1000);
        await Delay(.2f);
        Check(ExplorationRuntime.Instance.Cleared("D1") && !ExplorationRuntime.Instance.Cleared("D2"),"Only defeated guard group unlocks");
        Empty(); FieldInventory.Give(c.scrap,28); FieldInventory.Give(c.parts,6);
        var station=UnityEngine.Object.FindAnyObjectByType<FieldWorkbench>();
        player.transform.position=station.transform.position; Managers.Game.tower.transform.position=new Vector2(70,-70);
        await Delay(.2f); FieldHUD.Open(station);
        Check(FieldHUD.IsOpen,"Camp research panel opens");
        Check(!FieldSession.CanBuy(FieldResearch.All[0],28,6),"Research locked before first night");
        FieldSession.State.firstNightCleared=true;
        var panel=GameObject.Find("CampResearchPanel");
        var button=panel.transform.Find("U1").GetComponent<UnityEngine.UI.Button>();
        button.onClick.Invoke(); await Delay(.2f); player.transform.position+=Vector3.right;
        await Delay(.2f); Check(!FieldSession.State.Has("U1") && FieldInventory.Count(c.scrap)==28,"Movement cancels research without cost");
        player.transform.position=station.transform.position; await Delay(.2f);
        button.onClick.Invoke(); await Delay(3.3f);
        Check(FieldSession.State.Has("U1") && FieldInventory.Count(c.scrap)==22 && FieldInventory.Count(c.parts)==5,"U1 spends exactly 6 scrap and 1 part");
        button.onClick.Invoke(); await Delay(.2f);
        Check(FieldInventory.Count(c.scrap)==22,"Purchased research cannot be bought twice");
        var tower=Managers.Game.tower.GetComponent<Stat>(); tower.Hp=123;
        foreach(var id in new[]{"U2","U3","U4"}) { panel.transform.Find(id).GetComponent<UnityEngine.UI.Button>().onClick.Invoke(); await Delay(3.3f); }
        Check(FieldInventory.Count(c.scrap)==0 && FieldInventory.Count(c.parts)==0,"All research costs total 28 scrap and 6 parts");
        Check(tower.maxHP==650 && Math.Abs(tower.Hp-123)<.01f && FieldSession.CannonLimit==4,"U4 raises max HP and slots without healing");
        System.IO.File.WriteAllText("Logs/exploration-smoke.json",Newtonsoft.Json.JsonConvert.SerializeObject(checks,Newtonsoft.Json.Formatting.Indented));
        return string.Join("\n",checks);
    }
}
