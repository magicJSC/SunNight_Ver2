using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
public static class BalanceRegression
{
    static readonly List<string> checks=new List<string>();
    static void Check(bool ok,string label) { if(!ok) throw new Exception(label); checks.Add(label); Debug.Log("BALANCE_REGRESSION: "+label); }
    public static async Task<object> Run()
    {
        checks.Clear();
        var c=Resources.Load<ExplorationContent>("Exploration/Content");
        var piles=UnityEngine.Object.FindObjectsByType<TrashPileController>();
        Check(piles.SelectMany(x=>x.getItemList).Where(x=>x.item?.idName=="IronOre").Sum(x=>x.count)==7,"Legacy tutorial scrap total is 7; housing duplicates removed");
        Check(piles.SelectMany(x=>x.getItemList).Where(x=>x.item?.idName=="Steak").Sum(x=>x.count)==1,"Tutorial food total is 1");
        Check(!piles.SelectMany(x=>x.getItemList).Any(x=>x.item?.idName=="Cannon"),"No free tutorial cannons bypass crafting");
        FieldSession.NewRun();
        var totals=c.rewards.SelectMany(r=>r.items.Select((item,i)=>new{item,amount=r.amounts[i]})).GroupBy(x=>x.item).ToDictionary(x=>x.Key,x=>x.Sum(y=>y.amount));
        Check(totals["IronOre"]+7==71 && totals["MaintenancePart"]==6 && totals["Steak"]+1==4 && totals["HpPotion"]==5,"Complete field supply is 71 scrap / 6 parts / 4 food / 5 potions");
        Check(FieldResearch.All.Sum(x=>x.scrap)==28 && FieldResearch.All.Sum(x=>x.parts)==6,"All research costs 28 scrap and 6 parts");
        var tutorial=piles.Single(x=>x.name=="Trashfile (10)");
        int before=FieldInventory.Count(c.scrap);
        tutorial.SendMessage("GetRandomItem"); tutorial.SendMessage("GetRandomItem");
        Check(FieldInventory.Count(c.scrap)==before+4,"Repeated tutorial reward callback cannot duplicate scrap");
        Managers.Game.isCantPlay=false;
        Managers.Game.timeController.gameObject.SetActive(true);
        Managers.Game.player.GetComponent<PlayerStat>().Hunger=20;
        Managers.Game.player.GetComponent<PlayerStat>().Hp=73;
        float started=Time.time; await Task.Delay(2200);
        float elapsed=Time.time-started;
        Check(Mathf.Abs(Managers.Game.player.GetComponent<PlayerStat>().Hunger-(20-elapsed*.05f))<.04f,"Active first-day hunger drains at 0.05 per second");
        Check(Managers.Game.player.GetComponent<PlayerStat>().Hp==73,"Waiting does not regenerate field player health");
        foreach(var m in UnityEngine.Object.FindObjectsByType<MonsterController>()) UnityEngine.Object.Destroy(m.gameObject);
        Managers.Game.tower.transform.position=new Vector2(70,-70);
        Managers.Game.player.transform.position=new Vector2(58,-69);
        FieldSession.State.research.AddRange(new[]{"U2","U3"});
        ExplorationRuntime.GrantFirstNightReward();
        float clock=Managers.Game.timeController.TimeAmount;
        float hunger=Managers.Game.player.GetComponent<PlayerStat>().Hunger;
        await Task.Delay(1100);
        Check(clock==330 && Managers.Game.timeController.TimeAmount>clock && Managers.Game.timeController.day==2,"Day 2 starts at 05:30 and its clock advances");
        Check(Managers.Game.player.GetComponent<PlayerStat>().Hunger==hunger,"Post-night exploration does not consume hunger");
        MapManager.building.ClearAllTiles();
        MapManager.building.SetTile(new Vector3Int(1,0,0),Resources.Load<ItemSO>("Item/Cannon").buildTile);
        MapManager.building.SetTile(new Vector3Int(2,0,0),Resources.Load<ItemSO>("Item/Fence").buildTile);
        await Task.Delay(300);
        Managers.Game.player.GetComponent<PlayerStat>().Hp=73;
        Managers.Game.player.GetComponent<PlayerStat>().Hunger=14;
        Managers.Game.tower.GetComponent<Stat>().Hp=321;
        MapManager.building.GetInstantiatedObject(new Vector3Int(1,0,0)).GetComponent<Stat>().Hp=12;
        MapManager.building.GetInstantiatedObject(new Vector3Int(2,0,0)).GetComponent<Stat>().Hp=17;
        FieldCheckpoint.TransferPreparation=FieldCheckpoint.Capture();
        SceneManager.LoadScene("Boss1");
        await Task.Delay(4000);
        Check(FieldCheckpoint.TransferPreparation==null,"Factory transition consumes snapshot exactly once");
        Check(Managers.Game.tower.GetComponent<Stat>().Hp==321,"Factory retains damaged tower health");
        Check(Managers.Game.player.GetComponent<PlayerStat>().Hp<74 && Managers.Game.player.GetComponent<PlayerStat>().Hunger==14,"Factory retains player damage and hunger");
        var gun=MapManager.building.GetInstantiatedObject(new Vector3Int(1,0,0)).GetComponent<BuildStat>();
        var wall=MapManager.building.GetInstantiatedObject(new Vector3Int(2,0,0)).GetComponent<Stat>();
        Check(gun.Hp==12 && gun.maxHP==30 && gun.Damage==7 && wall.Hp==17,"Factory retains relative placement, damage, and research");
        var boss=UnityEngine.Object.FindAnyObjectByType<Boss1>();
        Check(boss.GetComponent<Stat>().Hp==1100,"Field Foxton health is 1100");
        boss.GetDamage(9); Check(boss.GetComponent<Stat>().Hp==1100,"Boss cannot be damaged before battle");
        boss.StartBattle(); boss.GetDamage(9);
        Check(boss.GetComponent<Stat>().Hp==1091,"Boss accepts player damage after battle starts");
        var cannon=gun.GetComponent<Cannon>(); cannon.AddTarget(boss.GetComponent<Collider2D>());
        Check(cannon.targets.Contains(boss.gameObject),"Cannon can acquire Foxton collider");
        System.IO.File.WriteAllText("Logs/BalanceAudit/regression.json",Newtonsoft.Json.JsonConvert.SerializeObject(checks,Newtonsoft.Json.Formatting.Indented));
        return checks;
    }
}
