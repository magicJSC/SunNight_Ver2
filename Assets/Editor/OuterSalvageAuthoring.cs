using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class OuterSalvageAuthoring
{
    public static void Sync(GameObject root,ExplorationContent c)
    {
        var outer=root.transform.Find(OuterFieldDressing.RootName);
        foreach(var old in c.outerSalvage ?? Array.Empty<FieldRewardDefinition>())
        {
            var previous=outer.Find(old.sceneryPath)?.GetComponent<SpriteRenderer>();
            if(previous!=null) previous.color=Color.white;
        }
        var clusters=outer.Cast<Transform>().SelectMany(z=>z.Cast<Transform>()).OrderBy(t=>t.name,StringComparer.Ordinal).ToArray();
        int budget=c.rewards.Where(d=>OuterSalvageLedger.Sources.Contains(d.id)).Sum(d=>
            Enumerable.Range(0,d.items.Length).Where(i=>d.items[i]=="IronOre").Sum(i=>d.amounts[i]))*OuterSalvageLedger.PiecesPerScrap;
        if(clusters.Length==0 || clusters.Length>budget || budget>clusters.Length*5)
            throw new Exception("Outer cluster count cannot fit guaranteed salvage budget");
        // Reserve one find for every cluster; put richer finds near each region's guarded destination.
        var anchors=new[]{new Vector2(82,144),new Vector2(-228,-99),new Vector2(112,-253),new Vector2(358,21)};
        var zones=new[]{"North_Woodland","West_Overgrowth","South_Salvage","East_Industrial"};
        var ordered=zones.Select((zone,z)=>clusters.Where(t=>t.parent.name==zone)
            .OrderBy(t=>Vector2.Distance(t.position,anchors[z])).ToArray()).ToArray();
        var amounts=clusters.ToDictionary(t=>t.name,t=>1);int remaining=budget-clusters.Length;
        for(int rank=0;remaining>0;rank++)
            for(int z=0;z<ordered.Length && remaining>0;z++)
                if(rank<ordered[z].Length) { int bonus=Math.Min(4,remaining);amounts[ordered[z][rank].name]+=bonus;remaining-=bonus; }
        c.outerSalvage=clusters.Select((cluster,i)=>
        {
            var prop=cluster.Cast<Transform>().Where(t=>t.name.StartsWith("Debris_"))
                // Put the readable interaction at the front of the cluster, below the tree canopy.
                .OrderBy(t=>t.GetComponent<SpriteRenderer>().bounds.center.y).First();
            var r=prop.GetComponent<SpriteRenderer>();
            r.color=new Color(1,.85f,.35f);
            return new FieldRewardDefinition { id="OS_"+cluster.name,region="조사 가능한 잔해",position=r.bounds.center,
                items=new[]{"IronOre"},amounts=new[]{amounts[cluster.name]},
                salvageFragments=true,sceneryPath=cluster.parent.name+"/"+cluster.name+"/"+prop.name,collectSeconds=1 };
        }).ToArray();
        EditorUtility.SetDirty(c);
    }
    public static object Build()
    {
        if(EditorApplication.isPlaying) throw new Exception("Stop Play Mode before authoring");
        const string path="Assets/LevelDesign/FirstField/FirstFieldDressing.prefab";
        var c=Resources.Load<ExplorationContent>("Exploration/Content");
        var previous=c.outerSalvage;
        var root=PrefabUtility.LoadPrefabContents(path);
        try
        {
            Sync(root,c);PrefabUtility.SaveAsPrefabAsset(root,path,out bool saved);
            if(!saved) throw new Exception("Outer salvage prefab could not be saved");
            AssetDatabase.SaveAssetIfDirty(c);
        }
        catch { c.outerSalvage=previous;EditorUtility.SetDirty(c);throw; }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        return new { success=true };
    }
}
