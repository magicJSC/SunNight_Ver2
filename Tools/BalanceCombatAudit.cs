using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
public static class BalanceCombatAudit
{
    public static async Task<object> Tutorial()
    {
        foreach(var m in UnityEngine.Object.FindObjectsByType<MonsterController>()) UnityEngine.Object.Destroy(m.gameObject);
        FieldSession.NewRun(); PlayerController.isDie=false;
        Managers.Game.isCantPlay=false; Managers.Game.timeController.gameObject.SetActive(false);
        Managers.Game.tower.transform.position=new Vector2(-33,-19);
        Managers.Game.player.transform.position=new Vector2(-35,-20);
        Managers.Game.player.GetComponent<PlayerStat>().Hp=100;
        ExplorationEffects.ApplyAll(); Managers.Game.tower.GetComponent<Stat>().Hp=500;
        MapManager.building.ClearAllTiles(); MapManager.buildData.Clear();
        MapManager.building.SetTile(new Vector3Int(0,1,0),Resources.Load<ItemSO>("Item/Cannon").buildTile);
        MapManager.building.SetTile(new Vector3Int(0,2,0),Resources.Load<ItemSO>("Item/Fence").buildTile);
        MapManager.building.SetTile(new Vector3Int(1,2,0),Resources.Load<ItemSO>("Item/Fence").buildTile);
        await Task.Delay(500);
        var defense=UnityEngine.Object.FindAnyObjectByType<DefenseEvent>();
        float start=Time.time; defense.SendMessage("StartDefense");
        while(!defense.open.activeSelf && !PlayerController.isDie && Time.time-start<180)
        {
            var p=Managers.Game.player;
            var near=UnityEngine.Object.FindObjectsByType<MonsterController>().OrderBy(x=>Vector2.Distance(x.transform.position,p.transform.position)).FirstOrDefault();
            p.SendMessage("SetMoveDirection",near!=null && Vector2.Distance(near.transform.position,p.transform.position)<3 ? ((Vector2)(p.transform.position-near.transform.position)).normalized : Vector2.zero);
            await Task.Delay(50);
        }
        Managers.Game.player.SendMessage("SetMoveDirection",Vector2.zero);
        var row=new{fixture=true,seconds=Time.time-start,cleared=defense.open.activeSelf,playerHp=Managers.Game.player.GetComponent<Stat>().Hp,towerHp=Managers.Game.tower.GetComponent<Stat>().Hp,cannons=ExplorationEffects.CannonCount()};
        System.IO.File.WriteAllText("Logs/BalanceAudit/tutorial-combat.json",Newtonsoft.Json.JsonConvert.SerializeObject(row,Newtonsoft.Json.Formatting.Indented));
        return row;
    }
    public static async Task<object> Defense(bool upgraded, int dogs, int koalas, int seed)
    {
        // Controlled fixture: no claim that these supplied structures were crafted during a playthrough.
        UnityEngine.Random.InitState(seed);
        foreach(var m in UnityEngine.Object.FindObjectsByType<MonsterController>()) UnityEngine.Object.Destroy(m.gameObject);
        FieldSession.NewRun();
        if(upgraded) FieldSession.State.research.AddRange(new[]{"U2","U3"});
        Managers.Game.isCantPlay=false; Managers.Game.isMovingTower=false;
        Managers.Game.timeController.gameObject.SetActive(false);
        Managers.Game.tower.transform.position=new Vector2(70,-70);
        PlayerController.isDie=false;
        Managers.Game.player.transform.position=new Vector2(68,-71);
        Managers.Game.player.GetComponent<PlayerStat>().Hp=100;
        ExplorationEffects.ApplyAll();
        Managers.Game.tower.GetComponent<Stat>().Hp=500;
        MapManager.building.ClearAllTiles(); MapManager.buildData.Clear();
        var cannon=Resources.Load<ItemSO>("Item/Cannon"); var wall=Resources.Load<ItemSO>("Item/Fence");
        foreach(var p in new[]{new Vector3Int(-1,1,0),new Vector3Int(1,1,0),new Vector3Int(1,-1,0)}) MapManager.building.SetTile(p,cannon.buildTile);
        foreach(var p in new[]{new Vector3Int(-1,2,0),new Vector3Int(0,2,0),new Vector3Int(1,2,0),new Vector3Int(2,-1,0),new Vector3Int(2,0,0),new Vector3Int(2,1,0)}) MapManager.building.SetTile(p,wall.buildTile);
        await Task.Delay(500);
        var wave=UnityEngine.Object.FindAnyObjectByType<MonsterWaveController>();
        var content=Resources.Load<ExplorationContent>("Exploration/Content");
        var test=ScriptableObject.CreateInstance<WaveSO>();
        test.groups=new List<WaveSO.Group> {
            new WaveSO.Group{monster=content.dog,count=dogs,spawnDelay=2,groupDelay=10},
            new WaveSO.Group{monster=content.coala,count=koalas,spawnDelay=2,groupDelay=0}};
        wave.waveSO=new[]{test};
        Managers.Game.timeController.day=1;
        float start=Time.time; wave.SendMessage("StartSpawn");
        while(!FieldSession.State.firstNightCleared && !PlayerController.isDie && Time.time-start<180)
        {
            var player=Managers.Game.player;
            var near=UnityEngine.Object.FindObjectsByType<MonsterController>().OrderBy(x=>Vector2.Distance(x.transform.position,player.transform.position)).FirstOrDefault();
            Vector2 move=Vector2.zero;
            if(near!=null && Vector2.Distance(near.transform.position,player.transform.position)<3)
                move=((Vector2)(player.transform.position-near.transform.position)).normalized;
            player.SendMessage("SetMoveDirection",move);
            await Task.Delay(50);
        }
        Managers.Game.player.SendMessage("SetMoveDirection",Vector2.zero);
        var row=new{fixture=true,policy="avoid enemies within 3; no player attacks",upgraded,dogs,koalas,seed,seconds=Time.time-start,cleared=FieldSession.State.firstNightCleared,
            towerHp=Managers.Game.tower.GetComponent<Stat>().Hp,playerHp=Managers.Game.player.GetComponent<PlayerStat>().Hp,
            surviving=UnityEngine.Object.FindObjectsByType<MonsterController>().Select(x=>new{x.name,hp=x.GetComponent<Stat>().Hp,pos=x.transform.position.ToString()}).ToArray(),
            buildings=UnityEngine.Object.FindObjectsByType<Item_Buliding>().Where(x=>x.transform.IsChildOf(MapManager.building.transform)).Select(x=>new{item=x.itemSo.idName,hp=x.GetComponent<Stat>().Hp}).ToArray()};
        var path="Logs/BalanceAudit/combat.json";
        var list=System.IO.File.Exists(path)?Newtonsoft.Json.Linq.JArray.Parse(System.IO.File.ReadAllText(path)):new Newtonsoft.Json.Linq.JArray();
        list.Add(Newtonsoft.Json.Linq.JObject.FromObject(row)); System.IO.File.WriteAllText(path,list.ToString());
        Debug.Log("BALANCE_COMBAT: "+Newtonsoft.Json.JsonConvert.SerializeObject(row));
        return row;
    }
}
