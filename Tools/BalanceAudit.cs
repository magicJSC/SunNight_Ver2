using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using Newtonsoft.Json;
public static class BalanceAudit
{
    public static readonly List<object> Events=new List<object>();
    public static object Snapshot(string phase)
    {
        var p=Managers.Game.player; var stat=p.GetComponent<PlayerStat>();
        var row=new {phase,time=Time.time,position=p.transform.position.ToString(),hp=stat.Hp,hunger=stat.Hunger,
            towerHp=Managers.Game.tower.GetComponent<Stat>().Hp,clock=Managers.Game.timeController?.TimeAmount,
            inventory=FieldInventory.Slots.Where(x=>x.slotInfo.itemInfo!=null).Select(x=>new {item=x.slotInfo.itemInfo.idName,x.slotInfo.count}).ToArray(),
            research=FieldSession.State.research.ToArray(),firstNight=FieldSession.State.firstNightCleared};
        string path="Logs/BalanceAudit/progress.json";
        var ledger=System.IO.File.Exists(path) ? Newtonsoft.Json.Linq.JArray.Parse(System.IO.File.ReadAllText(path)) : new Newtonsoft.Json.Linq.JArray();
        ledger.Add(Newtonsoft.Json.Linq.JObject.FromObject(row)); System.IO.File.WriteAllText(path,ledger.ToString()); return row;
    }
    public static object Inspect()
    {
        return new {
            ready=FieldInventory.Ready, frame=Time.frameCount, cantPlay=Managers.Game.isCantPlay,
            state=FieldInventory.Ready ? Snapshot("baseline") : null,
            pickups=UnityEngine.Object.FindObjectsByType<Item_Pick>().Select(x=>new {x.name,item=x.itemSo?.idName,count=x.Count,pos=x.transform.position.ToString()}).ToArray(),
            trash=UnityEngine.Object.FindObjectsByType<TrashPileController>().Select(x=>new{x.name,pos=x.transform.position.ToString(),drops=x.getItemList.Select(d=>new{item=d.item?.idName,d.count,d.probability}).ToArray()}).ToArray(),
            directors=UnityEngine.Object.FindObjectsByType<PlayableDirector>().Select(x=>new{x.name,state=x.state.ToString(),x.time,x.duration}).ToArray(),
            recipes=UnityEngine.Object.FindObjectsByType<UI_Produce>(FindObjectsInactive.Include).SelectMany(x=>x.toMakeItemList).Select(x=>new{item=x.toMakeItemSO.idName,cost=x.materialList.Select(m=>new{item=m.itemSO.idName,m.count}).ToArray()}).ToArray(),
            waves=UnityEngine.Object.FindObjectsByType<MonsterWaveController>().SelectMany(x=>x.waveSO.Take(1)).SelectMany(x=>x.groups).Select(x=>new{x.count,monster=x.monster.name,x.spawnDelay,x.groupDelay}).ToArray()
        };
    }
    public static async Task<object> Route(float[] xy)
    {
        for(int i=0;i<xy.Length;i+=2) { var result=await Walk(xy[i],xy[i+1]); if(Vector2.Distance(Managers.Game.player.transform.position,new Vector2(xy[i],xy[i+1]))>.5f) return result; }
        return Snapshot("route complete");
    }
    public static async Task<object> Collect(string objectName)
    {
        var go=GameObject.Find(objectName); if(go==null) throw new Exception("Missing "+objectName);
        var interact=Managers.Game.player.GetComponentInChildren<PlayerInteract>();
        if(!interact.interactObjectList.Contains(go)) return new {error="Outside interaction range",at=Managers.Game.player.transform.position.ToString()};
        interact.SetInteractObj(); interact.interactAction?.Invoke();
        await Task.Delay(5500); interact.cancelAction?.Invoke();
        return Snapshot("collect "+objectName);
    }
    public static async Task<object> Walk(float x,float y)
    {
        var player=Managers.Game.player; var target=new Vector2(x,y); var start=(Vector2)player.transform.position;
        float started=Time.time, lastMoved=Time.time; Vector2 last=start;
        try
        {
            while(Vector2.Distance(player.transform.position,target)>.25f)
            {
                if(PlayerController.isDie) throw new Exception("Player died");
                Vector2 current=player.transform.position;
                if(Vector2.Distance(last,current)>.1f) {last=current;lastMoved=Time.time;}
                if(Time.time-lastMoved>3 || Time.time-started>180) return new{blocked=true,at=current.ToString(),target=target.ToString(),seconds=Time.time-started};
                player.SendMessage("SetMoveDirection",(target-current).normalized);
                await Task.Delay(16);
            }
            return Snapshot("walk "+target);
        }
        finally { if(player!=null) player.SendMessage("SetMoveDirection",Vector2.zero); }
    }
}
