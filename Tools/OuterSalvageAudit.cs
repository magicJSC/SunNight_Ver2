using System;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using System.IO;
public static class OuterSalvageAudit
{
    public static object Contract()
    {
        var c=Resources.Load<ExplorationContent>("Exploration/Content");
        var field=typeof(ExplorationContent).GetField("outerSalvage");
        if(field==null) throw new Exception("No harvestable outer clusters registered");
        var entries=(Array)field.GetValue(c);
        if(entries==null || entries.Length!=156) throw new Exception("Each of 156 clusters must be harvestable");
        if(c.outerSalvage.Sum(d=>d.amounts[0])!=280) throw new Exception("Fragment budget must equal 28 scrap");
        int retained=c.rewards.Sum(d=>Enumerable.Range(0,d.items.Length)
            .Where(i=>d.items[i]=="IronOre" && !OuterSalvageLedger.Replaced(c,d,i)).Sum(i=>d.amounts[i]));
        if(retained+280/10!=64) throw new Exception("Global scrap changed");
        for(int prior=0;prior<=28;prior++)
        {
            var state=new ExplorationState{layoutVersion=3};int left=prior;
            foreach(var d in c.rewards.Where(d=>OuterSalvageLedger.Sources.Contains(d.id)))
            { int take=Math.Min(left,d.amounts[0]);state.AddClaim(d.id+":0",take);left-=take; }
            OuterSalvageLedger.Migrate(state,c);
            var serialized=JsonConvert.SerializeObject(state);
            OuterSalvageLedger.Migrate(state,c);
            if(JsonConvert.SerializeObject(state)!=serialized) throw new Exception("Migration not idempotent");
            int paid=0;
            foreach(var d in c.outerSalvage)
            {
                OuterSalvageLedger.TryCollect(state,c,d,n=>{paid+=n;return n;},out _,out _);
                if(OuterSalvageLedger.TryCollect(state,c,d,n=>{paid+=n;return n;},out _,out _))throw new Exception("Duplicate claim");
                state=JsonConvert.DeserializeObject<ExplorationState>(JsonConvert.SerializeObject(state));
            }
            if(paid+prior!=28) throw new Exception("Migration duplicates or loses scrap at "+prior);
        }
        var blocked=new ExplorationState{layoutVersion=4};
        foreach(var d in c.outerSalvage)
        {
            if((OuterSalvageLedger.Total(blocked,c)+d.amounts[0])/10>OuterSalvageLedger.Total(blocked,c)/10)
            {
                string before=JsonConvert.SerializeObject(blocked);
                if(OuterSalvageLedger.TryCollect(blocked,c,d,n=>0,out _,out _))throw new Exception("Full bag accepted");
                if(before!=JsonConvert.SerializeObject(blocked))throw new Exception("Full bag lost fragments");
                break;
            }
            OuterSalvageLedger.TryCollect(blocked,c,d,n=>n,out _,out _);
        }
        var result=new {clusters=entries.Length,pieces=280,totalScrap=64,migrationCases=29,saveRoundTrips=29*156,fullBagPreservesReward=true};
        File.WriteAllText("Logs/OuterSalvage/contract.json",JsonConvert.SerializeObject(result,Formatting.Indented));return result;
    }
    static async Task Load(string source)
    {
        FieldCheckpoint.RequestResume(source);SceneManager.LoadScene("TutorialScene");
        for(int i=0;i<200;i++)
        {await Task.Delay(100);if(ExplorationRuntime.Instance!=null && FieldInventory.Ready && GameObject.Find("Reward_OS_Island_001")!=null && !FieldSession.ResumeRequested)break;
            if(i==199)throw new Exception("Runtime timeout");}
        await Task.Delay(300);Managers.Game.isCantPlay=false;
    }
    public static async Task<object> Play()
    {
        await Load("docs/level-design/v03-postnight-earned-postnight.json");
        var c=ExplorationRuntime.Instance.content;
        var player=Managers.Game.player;
        var interaction=player.GetComponentInChildren<PlayerInteract>();
        int before=FieldInventory.Count(c.scrap),initial=OuterSalvageLedger.Total(FieldSession.State,c);
        var nodes=c.outerSalvage.Where(d=>FieldSession.State.Claimed(d.id+":0")<d.amounts[0]).OrderByDescending(d=>d.amounts[0]).Take(7).ToArray();
        foreach(var d in c.outerSalvage)
        {
            var obj=GameObject.Find("Reward_"+d.id);
            if(obj==null || obj.GetComponent<CircleCollider2D>()==null)throw new Exception("Missing interaction "+d.id);
        }
        // Fixture placement only: this test exercises interactions, not walking time or combat balance.
        var first=GameObject.Find("Reward_"+nodes[0].id).GetComponent<FieldReward>();
        player.transform.position=first.transform.position;Physics2D.SyncTransforms();await Task.Delay(200);
        first.ShowInteractSign();first.SetAction(interaction);interaction.interactAction?.Invoke();
        await Task.Delay(250);player.transform.position+=Vector3.right*3;await Task.Delay(1100);
        first.CancelAction(interaction);
        if(first.Empty || OuterSalvageLedger.Total(FieldSession.State,c)!=initial)throw new Exception("Movement failed to cancel");
        int collected=0;
        foreach(var d in nodes)
        {
            var target=GameObject.Find("Reward_"+d.id).GetComponent<FieldReward>();
            player.transform.position=target.transform.position;Physics2D.SyncTransforms();await Task.Delay(200);
            target.ShowInteractSign();target.SetAction(interaction);interaction.interactAction?.Invoke();await Task.Delay(1250);
            target.CancelAction(interaction);collected+=d.amounts[0];
            if(!target.Empty || !target.gameObject.activeSelf || target.transform.Find("SupplyMarker").gameObject.activeSelf)
                throw new Exception("Empty residue state incorrect "+d.id);
            if(target.transform.Find("Visual").GetComponent<SpriteRenderer>().color.r>.41f)throw new Exception("Residue not dimmed");
        }
        if(FieldInventory.Count(c.scrap)-before!=(initial+collected)/10-initial/10)throw new Exception("Actual inventory payout mismatch");
        int payout=FieldInventory.Count(c.scrap)-before;
        const string save="Logs/OuterSalvage/collected-save.json";
        FieldCheckpoint.Save(save);
        await Load(save);
        foreach(var d in nodes)if(!GameObject.Find("Reward_"+d.id).GetComponent<FieldReward>().Empty)throw new Exception("Save respawned loot");
        if(FieldInventory.Count(c.scrap)!=before+payout)throw new Exception("Save changed inventory");
        var focus=nodes[0].position;
        var result=new{runtimeNodes=c.outerSalvage.Length,collectedNodes=nodes.Length,collectedPieces=collected,scrap=payout,
            cancelledByMovement=true,emptyResidueVisible=true,markerHidden=true,reloaded=true,focusX=focus.x,focusY=focus.y};
        Debug.Log("OUTER_SALVAGE_VERIFIED: "+JsonConvert.SerializeObject(result));
        File.WriteAllText("Logs/OuterSalvage/play.json",JsonConvert.SerializeObject(result,Formatting.Indented));return result;
    }
    public static async Task<object> Preview()
    {
        await Load("docs/level-design/v03-postnight-earned-postnight.json");
        var nodes=ExplorationRuntime.Instance.content.outerSalvage;
        var first=nodes[0];
        var candidates=nodes.Where(d=>d.position.y>65 && d.position.x>0 && d.position.x<160).ToList();
        Vector2 p=new Vector2(70,65);var ids=new System.Collections.Generic.List<string>();
        for(int i=0;i<9 && candidates.Count>0;i++)
        {var d=candidates.OrderBy(x=>Vector2.Distance(p,x.position)).First();candidates.Remove(d);ids.Add(d.id);p=d.position;}
        var result=new{x=first.position.x,y=first.position.y,commands=string.Join(";",ids.Select(id=>"gather "+id))};
        File.WriteAllText("Logs/OuterSalvage/preview.json",JsonConvert.SerializeObject(result));return result;
    }
}


