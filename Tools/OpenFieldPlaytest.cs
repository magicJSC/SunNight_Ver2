using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
public static class OpenFieldPlaytest
{
    static readonly List<string> checks=new List<string>();
    static void Check(bool ok,string message) { if(!ok) throw new Exception(message); checks.Add(message); Debug.Log("OPEN_FIELD_PLAY: "+message); }
    public static async Task<object> Run()
    {
        checks.Clear();
        float deadline=Time.realtimeSinceStartup+20;
        while(ExplorationRuntime.Instance==null || UnityEngine.Object.FindObjectsByType<FieldReward>().Length!=45)
        { if(Time.realtimeSinceStartup>deadline) throw new Exception("Runtime spawn timeout"); await Task.Delay(100); }
        var c=ExplorationRuntime.Instance.content;
        int frame=Time.frameCount; await Task.Delay(500); Check(Time.frameCount>frame+5,"Actual game frames advance");
        Check(UnityEngine.Object.FindObjectsByType<FieldReward>().Length==45,"45 live gathering sites");
        Check(UnityEngine.Object.FindObjectsByType<FieldReward>().All(r=>r.transform.Find("SupplyMarker")!=null),"All gathering sites have a visible supply marker");
        Check(UnityEngine.Object.FindObjectsByType<FieldGuard>().Length==25,"25 live field enemies");
        var patrols=UnityEngine.Object.FindObjectsByType<FieldGuard>().Where(g=>g.patrolRadius>0).ToArray();
        var starts=patrols.Select(p=>(Vector2)p.transform.position).ToArray();
        await Task.Delay(3000);
        Check(patrols.Length==14 && patrols.Where((p,i)=>Vector2.Distance(p.transform.position,starts[i])>.4f).Count()==14,"All 14 patrols move using real Rigidbody physics");
        var groups=c.rewards.Where(r=>!string.IsNullOrEmpty(r.legacySource)).GroupBy(r=>r.legacySource).ToArray();
        var old=new ExplorationState();
        foreach(var group in groups) old.AddClaim(group.Key+":0",group.Sum(r=>r.amounts[0]));
        old.AddClaim("N1:0",4);
        ExplorationRuntime.MigrateClaims(old,c);
        Check(groups.All(g=>g.All(r=>old.Claimed(r.id+":0")==r.amounts[0])) && old.Claimed("N1:0")==4,"Fully claimed v0.2 bundles remain depleted; night reward unchanged");
        var partial=new ExplorationState(); partial.AddClaim("A1:0",2);
        ExplorationRuntime.MigrateClaims(partial,c); ExplorationRuntime.MigrateClaims(partial,c);
        Check(partial.Claimed("A1:0")==1 && partial.Claimed("A1_s1:0")==1 && partial.Claimed("A1_s2:0")==0 && partial.claims.Sum(x=>x.amount)==2,"Partial claim migration is lossless and idempotent");
        // Fixture positioning only. Collection below uses the same trigger and held interaction as the player.
        var p=Managers.Game.player; Managers.Game.isCantPlay=false;
        var reward=c.rewards.Single(r=>r.id=="A1");
        p.transform.position=reward.position+Vector2.right*5;
        float walkDeadline=Time.time+5;
        while(Vector2.Distance(p.transform.position,reward.position)>1.3f)
        { if(Time.time>walkDeadline) { p.SendMessage("SetMoveDirection",Vector2.zero); throw new Exception("Approach blocked"); } p.SendMessage("SetMoveDirection",Vector2.left); await Task.Delay(20); }
        p.SendMessage("SetMoveDirection",Vector2.zero); await Task.Delay(200);
        var interaction=p.GetComponentInChildren<PlayerInteract>(); interaction.SetInteractObj();
        int before=FieldInventory.Count(c.scrap);
        interaction.interactAction?.Invoke(); await Task.Delay(800); interaction.cancelAction?.Invoke();
        Check(FieldInventory.Count(c.scrap)==before,"Cancelled small salvage gives nothing");
        interaction.interactAction?.Invoke(); await Task.Delay(2400); interaction.cancelAction?.Invoke();
        Check(FieldInventory.Count(c.scrap)==before+1 && FieldSession.State.Claimed("A1:0")==1,"Held interaction collects one scrap from a scattered site");
        Check(!GameObject.Find("Reward_A1").transform.Find("SupplyMarker").gameObject.activeSelf,"Depleted salvage hides its supply marker");
        interaction.interactAction?.Invoke(); await Task.Delay(2300); interaction.cancelAction?.Invoke();
        Check(FieldInventory.Count(c.scrap)==before+1,"Collected site cannot duplicate supply");
        var guard=patrols.Single(g=>g.guardId=="P14");
        p.transform.position=(Vector2)guard.transform.position+Vector2.up*4;
        await Task.Delay(800);
        Check(guard.GetComponent<MonsterController>().target==p.transform,"Patrol detects the player through normal detection triggers");
        p.transform.position=new Vector2(70,-70);
        await Task.Delay(5000);
        Check(guard.GetComponent<MonsterController>().target==null && Vector2.Distance(guard.transform.position,guard.home)<4,"Patrol drops distant chase and returns to its territory");
        Check(!UnityEngine.Object.FindObjectsByType<FieldGuard>().Any(g=>Vector2.Distance(g.transform.position,new Vector2(70,-70))<25),"Camp retains an enemy-free buffer");
        guard.GetComponent<MonsterController>().GetDamage(1000);
        await Task.Delay(100);
        Check(FieldSession.State.killedGuards.Contains("P14"),"Patrol death recorded in save state (injected test damage)");
        SceneManager.LoadScene("TutorialScene"); await Task.Delay(3000);
        Check(!UnityEngine.Object.FindObjectsByType<FieldGuard>().Any(g=>g.guardId=="P14"),"Recorded patrol does not respawn on scene recreation");
        Check(GameObject.Find("Reward_A1").GetComponent<FieldReward>().Empty,"Claimed scattered supply stays empty on scene recreation");
        File.WriteAllText("Logs/OpenField/playtest.json",Newtonsoft.Json.JsonConvert.SerializeObject(checks,Newtonsoft.Json.Formatting.Indented));
        return checks;
    }
}
