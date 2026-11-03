using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using Object=UnityEngine.Object;

// Editor-only deterministic scenery. Interactions are registered separately in ExplorationRuntime.
public static class OuterFieldDressing
{
    const string Art="Assets/1.Asset/Sprite/Tile/PAL_SET1/";
    const string Deco="Assets/Resources/Prefabs/Map/Deco/";
    public const string RootName="OuterField";
    static bool Outer(Vector2 p)=>p.x < -118+12*Mathf.Sin(p.y*.035f) || p.x > 290+12*Mathf.Sin(p.y*.025f)
        || p.y > 65+12*Mathf.Sin(p.x*.03f) || p.y < -207+10*Mathf.Sin(p.x*.025f);
    public static Tilemap Ground()=>GameObject.Find("Grid/Tilemap")?.GetComponent<Tilemap>() ?? throw new Exception("Open TutorialScene with its field ground before authoring.");
    public static bool OnGround(Tilemap ground,Vector2 p,float margin)
    {
        foreach(var d in new[]{Vector2.zero,Vector2.up,Vector2.down,Vector2.left,Vector2.right,new Vector2(1,1),new Vector2(-1,1),new Vector2(1,-1),new Vector2(-1,-1)})
            if(!ground.HasTile(ground.WorldToCell(p+d*margin))) return false;
        return true;
    }
    static bool RewardSpace(Vector2 p,float radius,ExplorationContent c)=>c.rewards.Any(r=>Vector2.Distance(p,r.position)<radius+3);
    static bool Solid(Vector2 p,float radius)=>Physics2D.OverlapCircleAll(p,radius).Any(h=>!h.isTrigger && h.enabled && (h.attachedRigidbody==null || h.attachedRigidbody.bodyType==RigidbodyType2D.Static));
    public static void Redistribute(ExplorationContent c)
    {
        var positions=new Dictionary<string,Vector2>{
            {"D0_s2",new Vector2(-210,-90)},{"D0_s3",new Vector2(-222,-98)},
            {"D0_s4",new Vector2(95,-245)},{"D0_s5",new Vector2(108,-252)},
            {"E0",new Vector2(65,135)},{"E0_s1",new Vector2(78,142)},
            {"E0_s2",new Vector2(340,12)},{"E0_s3",new Vector2(353,18)},
            // Existing guarded supplies become outer destinations; IDs and amounts stay stable.
            {"D1",new Vector2(-228,-99)},{"D2",new Vector2(112,-253)},
            {"E1",new Vector2(82,144)},{"E2",new Vector2(358,21)}};
        foreach(var r in c.rewards) if(positions.TryGetValue(r.id,out var p))
        {
            r.position=p;
            r.region=p.x < -118?"서부 외곽 잔해":p.y < -207?"남부 야적장 잔해":p.y > 65?"북부 숲 잔해":"동부 산업 잔해";
        }
        var patrols=new Dictionary<string,Vector2>{
            // A roaming encounter on the approach, followed by the reward's existing defenders.
            {"P08",new Vector2(70,42)},{"P09",new Vector2(235,50)},
            {"P11",new Vector2(88,-145)},{"P12",new Vector2(-85,-81)},
            {"D1a",new Vector2(-210,-89)},{"D1b",new Vector2(-228,-108)},
            {"D2a",new Vector2(94,-242)},{"D2b",new Vector2(116,-258)},
            {"E1a",new Vector2(73,134)},{"E1b",new Vector2(87,145)},
            {"E2a",new Vector2(341,10)},{"E2b",new Vector2(362,24)}};
        foreach(var g in c.guards) if(patrols.TryGetValue(g.id,out var p)) g.position=p;
    }
    static Transform Child(Transform parent,string name,Vector2 p)
    {var g=new GameObject(name);g.transform.SetParent(parent,false);g.transform.position=p;return g.transform;}

    public static void Apply(GameObject root,Tilemap ground)
    {
        var previous=root.transform.Find(RootName);if(previous!=null) Object.DestroyImmediate(previous.gameObject);
        var c=Resources.Load<ExplorationContent>("Exploration/Content");
        var outer=Child(root.transform,RootName,Vector2.zero);
        var zones=new[]{"North_Woodland","West_Overgrowth","South_Salvage","East_Industrial"}.Select(n=>Child(outer,n,Vector2.zero)).ToArray();
        var material=AssetDatabase.LoadAssetAtPath<GameObject>(Deco+"StreetLamp.prefab").GetComponent<SpriteRenderer>().sharedMaterial;
        var trees=Enumerable.Range(1,3).Select(i=>AssetDatabase.LoadAllAssetsAtPath(Art+"Animated/tree"+i+".png").OfType<Sprite>().Single(s=>s.name=="tree"+i+"_0")).ToArray();
        string[][] palettes={new[]{"Firestone","Box","DrumCan","Chair1"},new[]{"Sofa","Chair1","Box","Tire2"},new[]{"DrumCan5","Tire","Box2","Locker"},new[]{"DrumCan","Tire2","Desk2","Box"}};
        var sprites=palettes.SelectMany(a=>a).Distinct().ToDictionary(n=>n,n=>AssetDatabase.LoadAssetAtPath<GameObject>(Deco+n+".prefab").GetComponent<SpriteRenderer>().sprite);
        var random=new System.Random(11022026);
        float Range(float a,float b)=>a+(float)random.NextDouble()*(b-a);
        int clusters=0;
        var centres=new List<Vector2>();
        for(int attempt=0;attempt<12000 && clusters<175;attempt++)
        {
            var p=new Vector2(Range(-332,419),Range(-290,248));
            if(centres.Any(q=>Vector2.Distance(q,p)<23)) continue;
            if(!Outer(p) || !OnGround(ground,p,12) || Solid(p,10)) continue;
            centres.Add(p);
            int zone=p.y>65?0:p.y < -207?2:p.x < -118?1:3;
            var cluster=Child(zones[zone],"Island_"+(++clusters).ToString("000"),p);
            int treeCount=zone<2?random.Next(4,9):random.Next(1,4);
            int propCount=random.Next(10,17);
            for(int i=0;i<propCount;i++)
            {
                bool tree=i<treeCount;
                Sprite sprite=tree?trees[random.Next(3)]:sprites[palettes[zone][random.Next(4)]];
                float size=tree?Range(4.5f,8):Range(.9f,2.8f);
                float scale=size/Mathf.Max(sprite.bounds.size.x,sprite.bounds.size.y);
                var centre=p+new Vector2(Range(-8,8),Range(-6,6));
                float radius=size*.6f;
                if(!OnGround(ground,centre,radius) || RewardSpace(centre,radius,c)) continue;
                var prop=Child(cluster,(tree?"Tree_":"Debris_")+i,centre);
                prop.localScale=Vector3.one*scale;
                prop.position-=(Vector3)(Vector2)sprite.bounds.center*scale;
                var r=prop.gameObject.AddComponent<SpriteRenderer>();r.sprite=sprite;r.sharedMaterial=material;r.sortingOrder=1;
                // Texture tint variation, keeping the source pixel artwork intact.
                r.color=tree?new Color(Range(.78f,1),Range(.85f,1),Range(.72f,.95f)):Color.white;
            }
        }
        Validate(root,ground);
        OuterSalvageAuthoring.Sync(root,c);
    }
    public static void Validate(GameObject root,Tilemap ground)
    {
        var outer=root.transform.Find(RootName);if(outer==null) throw new Exception("Outer field missing");
        var c=Resources.Load<ExplorationContent>("Exploration/Content");
        foreach(Transform zone in outer) if(zone.childCount<3) throw new Exception("Empty outer sector: "+zone.name);
        foreach(var component in outer.GetComponentsInChildren<Component>())
            if(!(component is Transform || component is SpriteRenderer)) throw new Exception("Unexpected outer component");
        foreach(var r in outer.GetComponentsInChildren<SpriteRenderer>())
        {
            if(r.sprite==null || r.sharedMaterial==null || !OnGround(ground,r.bounds.center,Mathf.Max(r.bounds.extents.x,r.bounds.extents.y))) throw new Exception("Invalid/off-ground scenery "+r.name);
            foreach(var reward in c.rewards)
                if(r.bounds.Intersects(new Bounds(reward.position,new Vector3(4,4,1)))) throw new Exception("Reward obscured: "+reward.id);
        }
    }
    [MenuItem("SunNight/Level Design/Rebuild Outer Field Dressing")]
    public static void Build()
    {
        if(Application.isPlaying) throw new Exception("Exit Play Mode before authoring");
        var ground=Ground();
        var c=Resources.Load<ExplorationContent>("Exploration/Content");
        Redistribute(c);ExplorationContentBuilder.Validate(c);
        foreach(var r in c.rewards) if(!OnGround(ground,r.position,1) || Solid(r.position,.7f)) throw new Exception("Invalid resource position: "+r.id);
        foreach(var g in c.guards) if(!OnGround(ground,g.position,5) || Solid(g.position,4)) throw new Exception("Invalid patrol position: "+g.id);
        EditorUtility.SetDirty(c);AssetDatabase.SaveAssetIfDirty(c);
        var root=PrefabUtility.LoadPrefabContents(FirstFieldLevelDesign.PrefabPath);
        var previous=c.outerSalvage;
        try
        {
            Apply(root,ground);PrefabUtility.SaveAsPrefabAsset(root,FirstFieldLevelDesign.PrefabPath,out bool saved);
            if(!saved) throw new Exception("Outer field prefab could not be saved");
            AssetDatabase.SaveAssetIfDirty(c);
        }
        catch { c.outerSalvage=previous;EditorUtility.SetDirty(c);throw; }
        finally {PrefabUtility.UnloadPrefabContents(root);}
    }
}
