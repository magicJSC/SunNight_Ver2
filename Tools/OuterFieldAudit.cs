using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using System.Threading.Tasks;

public static class OuterFieldAudit
{
    public static async Task<object> Play()
    {
        for(int i=0;ExplorationRuntime.Instance==null || !FieldInventory.Ready;i++)
        {if(i>150)throw new Exception("Runtime not ready");await Task.Delay(100);}
        await Task.Delay(400);
        var c=ExplorationRuntime.Instance.content;
        string[] ids={"D0_s2","D0_s3","D0_s4","D0_s5","E0","E0_s1","E0_s2","E0_s3"};
        foreach(var id in ids)
        {
            var obj=GameObject.Find("Reward_"+id);var def=c.rewards.Single(r=>r.id==id);
            if(obj==null || Vector2.Distance(obj.transform.position,def.position)>.01f)throw new Exception("Missing/misplaced reward: "+id);
        }
        string[] guards={"P08","P09","P11","P12"};
        var patrols=guards.Select(id=>UnityEngine.Object.FindObjectsByType<FieldGuard>().Single(g=>g.guardId==id)).ToArray();
        foreach(var g in patrols) if(Vector2.Distance(g.home,c.guards.Single(d=>d.id==g.guardId).position)>.01f)throw new Exception("Patrol home changed");
        Managers.Game.isCantPlay=false;
        var first=patrols.Select(g=>g.transform.position).ToArray();
        await Task.Delay(2600);
        if(patrols.Where((g,i)=>Vector2.Distance(g.transform.position,first[i])<.1f).Any())throw new Exception("Outer patrol did not move");
        var player=Managers.Game.player;var beforePosition=player.transform.position;
        var target=GameObject.Find("Reward_D0_s2").GetComponent<FieldReward>();
        player.transform.position=target.transform.position+Vector3.down;
        Physics2D.SyncTransforms();await Task.Delay(250);
        var interaction=player.GetComponentInChildren<PlayerInteract>();
        int before=FieldInventory.Count(c.scrap);
        target.ShowInteractSign();target.SetAction(interaction);interaction.interactAction?.Invoke();
        await Task.Delay(2600);target.CancelAction(interaction);
        if(FieldInventory.Count(c.scrap)!=before+1 || !target.Empty)throw new Exception("Outer resource collection failed");
        target.ShowInteractSign();target.SetAction(interaction);interaction.interactAction?.Invoke();await Task.Delay(2200);target.CancelAction(interaction);
        if(FieldInventory.Count(c.scrap)!=before+1)throw new Exception("Duplicate resource granted");
        player.transform.position=beforePosition;
        var result=new{outerRewardSpawns=ids.Length,patrolHomes=guards.Length,patrolMovement=true,collectedScrap=1,duplicateBlocked=true,
            runtimeRewards=UnityEngine.Object.FindObjectsByType<FieldReward>().Length,runtimeGuards=UnityEngine.Object.FindObjectsByType<FieldGuard>().Length};
        File.WriteAllText("Logs/OuterField/play.json",JsonConvert.SerializeObject(result,Formatting.Indented));
        Debug.Log("OUTER_FIELD_VERIFIED: 8 reward positions, 4 patrol homes/movement, actual collection and duplicate prevention.");
        return result;
    }
    public static object Rebuild() { OuterFieldDressing.Build();return Validate(); }
    public static object Connectivity()
    {
        var ground=OuterFieldDressing.Ground();
        bool Solid(Collider2D h)=>!h.isTrigger && h.enabled && (h.attachedRigidbody==null || h.attachedRigidbody.bodyType==RigidbodyType2D.Static);
        bool Clear(Vector2 a,Vector2 b)=>!Physics2D.CircleCastAll(a,.7f,(b-a).normalized,Vector2.Distance(a,b)).Any(h=>Solid(h.collider));
        foreach(var t in UnityEngine.Object.FindObjectsByType<TilemapCollider2D>()) t.ProcessTilemapChanges();
        Physics2D.SyncTransforms();
        var seen=new System.Collections.Generic.HashSet<Vector2Int>();var tested=new System.Collections.Generic.HashSet<Vector2Int>();
        var queue=new System.Collections.Generic.Queue<Vector2Int>();var start=new Vector2Int(18,-18);seen.Add(start);tested.Add(start);queue.Enqueue(start);
        while(queue.Count>0)
        {
            var p=queue.Dequeue();
            foreach(var d in new[]{Vector2Int.up,Vector2Int.down,Vector2Int.left,Vector2Int.right})
            {
                var q=p+d;if(tested.Contains(q))continue;
                Vector2 end=(Vector2)q*4;
                if(!OuterFieldDressing.OnGround(ground,end,.7f) || Physics2D.OverlapCircleAll(end,.7f).Any(Solid)){tested.Add(q);continue;}
                if(!Clear((Vector2)p*4,end))continue;
                tested.Add(q);seen.Add(q);queue.Enqueue(q);
            }
        }
        var c=Resources.Load<ExplorationContent>("Exploration/Content");
        var unreachable=c.rewards.Where(r=>!seen.Any(p=>Vector2.Distance((Vector2)p*4,r.position)<6 && Clear((Vector2)p*4,r.position))).Select(r=>r.id).ToArray();
        var result=new{spacing=4,bodyRadius=.7f,reachableCells=seen.Count,rewards=c.rewards.Length,unreachable};
        File.WriteAllText("Logs/OuterField/connectivity.json",JsonConvert.SerializeObject(result,Formatting.Indented));
        if(unreachable.Length>0)throw new Exception("Unreachable rewards: "+string.Join(",",unreachable));return result;
    }
    public static object Build()
    {
        var c=Resources.Load<ExplorationContent>("Exploration/Content");
        File.WriteAllText("Logs/OuterField/content-before.json",JsonUtility.ToJson(c,true));
        OuterFieldDressing.Build();
        return Validate();
    }
    public static object Validate()
    {
        var root=GameObject.Find("FirstFieldDressing");
        OuterFieldDressing.Validate(root,OuterFieldDressing.Ground());
        var c=Resources.Load<ExplorationContent>("Exploration/Content");ExplorationContentBuilder.Validate(c);
        File.WriteAllText("Logs/OuterField/content-after.json",JsonUtility.ToJson(c,true));
        var outer=root.transform.Find(OuterFieldDressing.RootName);
        var result=new {scene=SceneManager.GetActiveScene().path,sceneDirty=SceneManager.GetActiveScene().isDirty,
            sprites=outer.GetComponentsInChildren<SpriteRenderer>().Length,
            sectors=outer.Cast<Transform>().Select(t=>new{t.name,clusters=t.childCount,sprites=t.GetComponentsInChildren<SpriteRenderer>().Length}).ToArray(),
            colliders=outer.GetComponentsInChildren<Collider2D>().Length,rewards=c.rewards.Length,guards=c.guards.Length};
        File.WriteAllText("Logs/OuterField/validation.json",JsonConvert.SerializeObject(result,Formatting.Indented));return result;
    }
    public static object Inspect()
    {
        var scene=SceneManager.GetActiveScene();
        var result=new {scene=scene.path,scene.isDirty,playing=Application.isPlaying,
            maps=UnityEngine.Object.FindObjectsByType<Tilemap>().Select(m=>new{name=m.name,path=PathOf(m.transform),bounds=m.cellBounds.ToString(),position=m.transform.position.ToString(),scale=m.transform.lossyScale.ToString(),tiles=m.GetUsedTilesCount()}).ToArray(),
            roots=scene.GetRootGameObjects().Select(g=>new{g.name,position=g.transform.position.ToString()}).ToArray()};
        Directory.CreateDirectory("Logs/OuterField");
        File.WriteAllText("Logs/OuterField/before.json",JsonConvert.SerializeObject(result,Formatting.Indented));
        return result;
    }
    static string PathOf(Transform t)=>t.parent==null?t.name:PathOf(t.parent)+"/"+t.name;
}
