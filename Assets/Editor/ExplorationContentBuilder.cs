using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class ExplorationContentBuilder
{
    [MenuItem("SunNight/Level Design/Build Exploration Content")]
    public static void Build()
    {
        if(!AssetDatabase.IsValidFolder("Assets/Resources/Exploration")) AssetDatabase.CreateFolder("Assets/Resources","Exploration");
        var scrap=Resources.Load<ItemSO>("Item/IronOre");
        const string partPath="Assets/Resources/Item/MaintenancePart.asset";
        var parts=AssetDatabase.LoadAssetAtPath<ItemSO>(partPath);
        if(parts==null) { parts=ScriptableObject.CreateInstance<ItemSO>(); AssetDatabase.CreateAsset(parts,partPath); }
        parts.idName="MaintenancePart"; parts.itemName="정비 부품"; parts.explain="야영지 연구에 사용하는 정밀 부품. 경비 보상 상자에서 획득합니다.";
        parts.itemType=Define.ItemType.Pick; parts.maxAmount=64; parts.itemIcon=scrap.itemIcon; parts.canSmelt=false;
        var pickup=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/Items/IronOre"));
        pickup.name="MaintenancePart"; pickup.GetComponent<Item>().itemSo=parts;
        parts.itemPrefab=PrefabUtility.SaveAsPrefabAsset(pickup,"Assets/Resources/Prefabs/Items/MaintenancePart.prefab");
        UnityEngine.Object.DestroyImmediate(pickup); EditorUtility.SetDirty(parts);
        const string path="Assets/Resources/Exploration/Content.asset";
        var c=AssetDatabase.LoadAssetAtPath<ExplorationContent>(path);
        if(c==null) { c=ScriptableObject.CreateInstance<ExplorationContent>(); AssetDatabase.CreateAsset(c,path); }
        c.scrap=scrap; c.parts=parts; c.food=Resources.Load<ItemSO>("Item/Steak"); c.potion=Resources.Load<ItemSO>("Item/HpPotion");
        var box=Resources.Load<GameObject>("Prefabs/Map/Deco/Box").GetComponent<SpriteRenderer>();
        c.food.hungerAmount=12; EditorUtility.SetDirty(c.food);
        c.chest=box.sprite; c.material=box.sharedMaterial;
        c.salvage=AssetDatabase.LoadAllAssetsAtPath("Assets/1.Asset/Sprite/Tile/PAL_SET1/dec_props.png").OfType<Sprite>().Single(x=>x.name=="dec_props_37");
        c.workbench=Resources.Load<GameObject>("Prefabs/Map/Deco/Desk").GetComponent<SpriteRenderer>().sprite;
        c.font=AssetDatabase.LoadAssetAtPath<Font>("Assets/1.Asset/Font/Galmuri9.ttf");
        c.dog=Resources.Load<GameObject>("Prefabs/Monster/Dog"); c.coala=Resources.Load<GameObject>("Prefabs/Monster/Coala");
        var rewards=new List<FieldRewardDefinition>();
        void Reward(string id,string region,float x,float y,string group,string[] items,int[] amounts,bool night=false)
        { rewards.Add(new FieldRewardDefinition { id=id,region=region,position=new Vector2(x,y),group=group,items=items,amounts=amounts,requiresFirstNight=night }); }
        void Scrap(string id,string region,float x,float y,int amount,string group="") => Reward(id,region,x,y,group,new[]{"IronOre"},new[]{amount});
        void Food(string id,string region,float x,float y) => Reward(id,region,x,y,"",new[]{"Steak"},new[]{1});
        // Stable parent IDs allow old full/partial claims to migrate to the scattered pieces.
        void Scatter(string source,string region,params float[] xy)
        {
            for(int i=0;i<xy.Length;i+=2)
            {
                Scrap(i==0 ? source : source+"_s"+(i/2),region,xy[i],xy[i+1],1);
                rewards[rewards.Count-1].legacySource=source;
                rewards[rewards.Count-1].collectSeconds=2;
            }
        }
        // Keep these loose deposits discoverable in a 4:3 gameplay viewport without drawing a path.
        Scatter("A1","서쪽 공터 고철",-24,-57, -47,-67, -13,-50);
        Scatter("A2","남쪽 공터 고철",1,-78, 24,-88, 9,-105);
        Scatter("A3","북쪽 공터 고철",28,-51, 55,-35, 62,-15);
        Food("A4","공터 식량",-4,-65);
        Scatter("B1","폐주거지 북쪽 잔해",-8,-119, 19,-134, 35,-113);
        Scatter("B2","폐주거지 동쪽 잔해",9,-151, 31,-167, 52,-145);
        Scatter("B3","폐주거지 남쪽 잔해",-6,-182, 21,-191, 53,-177);
        Food("B4","폐주거지 식량",12,-171);
        Scatter("C1","숲 북쪽 잔해",-75,-94, -93,-76, -60,-105);
        Scatter("C2","숲 남쪽 잔해",-90,-127, -65,-148, -95,-169);
        Food("C3","숲 야영 흔적",-73,-135);
        Scatter("D0","남동부 고철 잔해",94,-116, 107,-154, 124,-182, 153,-183, 187,-176, 201,-144);
        Reward("D1","D 서쪽 경비 보상",121,-157,"D1",new[]{"IronOre","MaintenancePart"},new[]{6,1});
        Reward("D2","D 동쪽 경비 보상",181,-159,"D2",new[]{"IronOre","MaintenancePart","HpPotion"},new[]{6,1,1});
        Scatter("E0","북동부 기계 잔해",91,0, 101,47, 159,46, 181,13);
        Reward("E1","E 서쪽 작업장",107,21,"E1",new[]{"IronOre","MaintenancePart","HpPotion"},new[]{4,2,1});
        Reward("E2","E 동쪽 작업장",145,19,"E2",new[]{"IronOre","MaintenancePart","HpPotion"},new[]{4,2,1});
        Scrap("F1","F 공장 외곽",235,-70,3,"F"); Scrap("F2","F 공장 외곽",239,-63,3,"F");
        Reward("F3","F 공장 외곽",233,-78,"F",new[]{"HpPotion"},new[]{1});
        Reward("N1","첫밤 완료 보급",58,-74,"",new[]{"IronOre","HpPotion"},new[]{4,1},true);
        c.rewards=rewards.ToArray();
        var guards=new List<FieldGuardDefinition>();
        void Guard(string id,string group,float x,float y,bool ranged=false) => guards.Add(new FieldGuardDefinition { id=id,group=group,position=new Vector2(x,y),ranged=ranged });
        Guard("D1a","D1",119,-164); Guard("D1b","D1",122,-166);
        Guard("D2a","D2",181,-167); Guard("D2b","D2",184,-169);
        Guard("E1a","E1",108,26); Guard("E1b","E1",110,30,true);
        Guard("E2a","E2",147,25); Guard("E2b","E2",149,29,true);
        Guard("Fa","F",222,-87); Guard("Fb","F",226,-89); Guard("Fc","F",230,-89,true);
        void Patrol(string id,float x,float y,bool ranged=false)
        {
            Guard(id,"",x,y,ranged);
            guards[guards.Count-1].patrolRadius=3;
        }
        Patrol("P01",-82,-55); Patrol("P02",-97,-149); Patrol("P03",-45,-115);
        // First-day traversal: bring one existing patrol into the empty residential farming area.
        // It remains avoidable, with a 12-unit home leash and no camp overlap.
        Patrol("P04",20,-145); Patrol("P05",76,-172); Patrol("P06",53,20);
        Patrol("P07",119,51); Patrol("P08",173,34,true); Patrol("P09",192,-27);
        Patrol("P10",180,-84,true); Patrol("P11",218,-137); Patrol("P12",110,-188);
        Patrol("P13",-42,-185); Patrol("P14",18,-23);
        c.guards=guards.ToArray(); OuterFieldDressing.Redistribute(c); EditorUtility.SetDirty(c); AssetDatabase.SaveAssets();
        Validate(c);
    }
    public static void Validate(ExplorationContent c)
    {
        if(c==null || c.font==null || c.parts.itemPrefab==null || c.dog==null || c.coala==null) throw new Exception("Missing exploration asset");
        if(c.rewards.Select(x=>x.id).Distinct().Count()!=c.rewards.Length || c.guards.Select(x=>x.id).Distinct().Count()!=c.guards.Length) throw new Exception("Duplicate persistent ID");
        var totals=new Dictionary<string,int>();
        foreach(var r in c.rewards)
        {
            if(r.items.Length!=r.amounts.Length) throw new Exception("Invalid reward");
            if(!string.IsNullOrEmpty(r.group) && !c.guards.Any(g=>g.group==r.group)) throw new Exception("Unreachable combat reward");
            for(int i=0;i<r.items.Length;i++)
            { if(c.Item(r.items[i])==null || r.amounts[i]<=0) throw new Exception("Invalid item"); totals.TryGetValue(r.items[i],out int old); totals[r.items[i]]=old+r.amounts[i]; }
        }
        if(totals["IronOre"]!=64 || totals["MaintenancePart"]!=6 || totals["Steak"]!=3 || totals["HpPotion"]!=5) throw new Exception("v0.2 field supply mismatch");
        if(c.guards.Length!=25 || c.rewards.Length!=45) throw new Exception("Open field population mismatch");
        if(c.guards.Any(x=>Vector2.Distance(x.position,new Vector2(70,-70))<30)) throw new Exception("Enemy inside camp buffer");
        if(c.rewards.Any(x=>x.collectSeconds<=0)) throw new Exception("Invalid collection time");
        Debug.Log("EXPLORATION_CONTENT_OK: v0.3 field supplies=64/6/3/5; 45 gathering sites; 11 guards + 14 patrols.");
    }
}
