using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
public static class OpenFieldAudit
{
    static void Check(bool ok,string message) { if(!ok) throw new Exception(message); Debug.Log("OPEN_FIELD_CHECK: "+message); }
    static int Tiles(Tilemap map) { int count=0; foreach(var p in map.cellBounds.allPositionsWithin) if(map.HasTile(p)) count++; return count; }
    public static object Build()
    {
        Check(!Application.isPlaying,"Editor authoring mode");
        string sceneHash=Hash(FirstFieldLevelDesign.ScenePath);
        var old=AssetDatabase.LoadAssetAtPath<GameObject>(FirstFieldLevelDesign.PrefabPath);
        int before=old.GetComponentsInChildren<Tilemap>().Sum(Tiles);
        ExplorationContentBuilder.Build(); FirstFieldLevelDesign.Build();
        EditorSceneManager.OpenScene(FirstFieldLevelDesign.ScenePath);
        var root=GameObject.Find("FirstFieldDressing");
        FirstFieldLevelDesign.Validate(root);
        var c=Resources.Load<ExplorationContent>("Exploration/Content");
        foreach(var collider in UnityEngine.Object.FindObjectsByType<TilemapCollider2D>()) collider.ProcessTilemapChanges();
        Physics2D.SyncTransforms();
        bool Blocked(Vector2 p)=>Physics2D.OverlapCircleAll(p,.7f).Any(h=>!h.isTrigger && h.enabled && h.gameObject.activeInHierarchy && (h.attachedRigidbody==null || h.attachedRigidbody.bodyType==RigidbodyType2D.Static));
        var pickupChecks=c.rewards.Select(r=>new{id=r.id,x=r.position.x,y=r.position.y,r.region,r.items,r.amounts,r.group,r.collectSeconds,blocked=Blocked(r.position),approaches=new[]{Vector2.up,Vector2.down,Vector2.left,Vector2.right}.Count(d=>!Blocked(r.position+d*3))}).ToArray();
        var patrolChecks=c.guards.Select(g=>new{id=g.id,x=g.position.x,y=g.position.y,g.group,g.ranged,g.patrolRadius,blocked=Enumerable.Range(0,13).Any(i=>Blocked(g.position+Vector2.right*g.patrolRadius*(i/6f-1)))}).ToArray();
        var report=new {referenceV02GroundTiles=4588,groundTiles=root.GetComponentsInChildren<Tilemap>().Sum(Tiles),roadTiles=Tiles(root.GetComponentsInChildren<Tilemap>().Single(x=>x.name=="BrokenRoadRemnants")),props=root.GetComponentsInChildren<SpriteRenderer>().Length,
            rewards=c.rewards.Length,guards=c.guards.Length,patrols=c.guards.Count(g=>g.patrolRadius>0),sceneUnchanged=sceneHash==Hash(FirstFieldLevelDesign.ScenePath),pickupChecks,patrolChecks};
        File.WriteAllText("Logs/OpenField/layout-audit.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));
        Check(pickupChecks.All(x=>!x.blocked && x.approaches>=3),"Every gathering site has a clear centre and at least three approaches");
        Check(patrolChecks.All(x=>!x.blocked),"All guard spawns and patrol sweeps clear static colliders");
        Check(report.sceneUnchanged,"Legacy scene bytes unchanged");
        return report;
    }
    static string Hash(string path) { using(var sha=System.Security.Cryptography.SHA256.Create()) return Convert.ToBase64String(sha.ComputeHash(File.ReadAllBytes(path))); }
    public static object Connectivity()
    {
        bool Solid(Collider2D h)=>!h.isTrigger && h.enabled && h.gameObject.activeInHierarchy && (h.attachedRigidbody==null || h.attachedRigidbody.bodyType==RigidbodyType2D.Static);
        bool Blocked(Vector2 p)=>Physics2D.OverlapCircleAll(p,.7f).Any(Solid);
        bool Clear(Vector2 a,Vector2 b)=>!Physics2D.CircleCastAll(a,.7f,(b-a).normalized,Vector2.Distance(a,b)).Any(h=>Solid(h.collider));
        const int width=196,height=134;
        Vector2 Point(Vector2Int cell)=>new Vector2(-110+cell.x*2,-200+cell.y*2);
        var seen=new HashSet<Vector2Int>(); var visited=new HashSet<Vector2Int>(); var queue=new Queue<Vector2Int>();
        var start=new Vector2Int(90,65); queue.Enqueue(start); visited.Add(start);
        while(queue.Count>0)
        {
            var cell=queue.Dequeue(); seen.Add(cell);
            foreach(var d in new[]{Vector2Int.up,Vector2Int.down,Vector2Int.left,Vector2Int.right})
            {
                var next=cell+d;
                if(next.x<0 || next.y<0 || next.x>=width || next.y>=height || visited.Contains(next)) continue;
                if(Blocked(Point(next))) { visited.Add(next); continue; }
                if(!Clear(Point(cell),Point(next))) continue;
                visited.Add(next); queue.Enqueue(next);
            }
        }
        var c=Resources.Load<ExplorationContent>("Exploration/Content");
        var unreachable=c.rewards.Where(r=>!seen.Any(cell=>Vector2.Distance(Point(cell),r.position)<3 && Clear(Point(cell),r.position))).Select(r=>r.id).ToArray();
        var report=new{sampleSpacing=2,bodyRadius=.7f,reachableCells=seen.Count,reachableRewards=c.rewards.Length-unreachable.Length,unreachable};
        File.WriteAllText("Logs/OpenField/connectivity.json",Newtonsoft.Json.JsonConvert.SerializeObject(report,Newtonsoft.Json.Formatting.Indented));
        Check(unreachable.Length==0,"All 45 sites reachable from camp across free space with swept collision checks");
        return report;
    }
    public static async Task<object> Capture(string label,float x,float y,float size)
    {
        // Temporary camera, no scene save. Gameplay UI is hidden only in this camera's culling mask.
        var go=new GameObject("OpenFieldPreview");
        var cam=go.AddComponent<Camera>(); cam.enabled=false; cam.orthographic=true; cam.orthographicSize=size;
        cam.transform.position=new Vector3(x,y,-10); cam.clearFlags=CameraClearFlags.SolidColor; cam.backgroundColor=new Color(.08f,.06f,.04f);
        cam.cullingMask=~(1<<LayerMask.NameToLayer("UI"));
        cam.transparencySortMode=TransparencySortMode.CustomAxis; cam.transparencySortAxis=Vector3.up;
        var data=go.AddComponent<UniversalAdditionalCameraData>(); data.renderPostProcessing=false; data.SetRenderer(-1);
        var others=UnityEngine.Object.FindObjectsByType<Light2D>().Where(l=>l.enabled && l.lightType==Light2D.LightType.Global).ToArray();
        foreach(var l in others) l.enabled=false;
        var light=go.AddComponent<Light2D>(); light.lightType=Light2D.LightType.Global; light.intensity=1; light.color=new Color(.8f,.8f,.8f);
        light.targetSortingLayers=new[]{SortingLayer.NameToID("Default")};
        var rt=new RenderTexture(1600,1000,24); rt.filterMode=FilterMode.Point; rt.Create(); cam.targetTexture=rt;
        try
        {
            await Task.Delay(400);
            var request=new UniversalRenderPipeline.SingleCameraRequest { destination=rt };
            if(RenderPipelineManager.currentPipeline==null) cam.Render();
            RenderPipeline.SubmitRenderRequest(cam,request); await Task.Delay(100); RenderPipeline.SubmitRenderRequest(cam,request);
            var previous=RenderTexture.active; RenderTexture.active=rt;
            var tex=new Texture2D(1600,1000,TextureFormat.RGB24,false);
            tex.ReadPixels(new Rect(0,0,1600,1000),0,0); tex.Apply();
            File.WriteAllBytes("Logs/OpenField/"+label+".png",tex.EncodeToPNG());
            RenderTexture.active=previous; UnityEngine.Object.DestroyImmediate(tex);
        }
        finally { UnityEngine.Object.DestroyImmediate(go); foreach(var l in others) if(l!=null) l.enabled=true; rt.Release(); UnityEngine.Object.DestroyImmediate(rt); }
        return label;
    }
}
