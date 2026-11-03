using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable] public sealed class FieldSlot { public string item; public int count; }
[Serializable] public sealed class FieldBuilding { public int x,y; public string item; public float hp; }
[Serializable] public sealed class FieldSave
{
    public int version=1;
    public ExplorationState state;
    public FieldSlot[] slots;
    public List<FieldBuilding> buildings = new List<FieldBuilding>();
    public float playerX,playerY,towerX,towerY,hp,hunger,towerHp,time;
    public int day,coin;
}
public static class FieldCheckpoint
{
    public static string PathName => Path.Combine(Application.persistentDataPath,"exploration-v02.json");
    public static bool Exists => File.Exists(PathName);
    public static FieldSlot[] Transfer;
    public static FieldSave TransferPreparation;
    static string resumeSource;
    public static void RequestResume(string source = null)
    {
        Read(Resources.Load<ExplorationContent>("Exploration/Content"),source);
        FieldSession.NewRun(); FieldSession.ResumeRequested=true; resumeSource=source;
    }
    public static FieldSlot[] CaptureInventory() => FieldInventory.Slots.Select(x=>new FieldSlot { item=x.slotInfo.itemInfo?.idName,count=x.slotInfo.count }).ToArray();
    public static FieldSave Capture()
    {
        var game=Managers.Game;
        var player=game.player.GetComponent<PlayerStat>();
        var data=new FieldSave { state=FieldSession.State,slots=CaptureInventory(),hp=player.Hp,hunger=player.Hunger,
            playerX=player.transform.position.x,playerY=player.transform.position.y,towerX=game.tower.transform.position.x,
            towerY=game.tower.transform.position.y,towerHp=game.tower.GetComponent<Stat>().Hp,
            day=game.timeController.day,time=game.timeController.TimeAmount,coin=Managers.Inven.Coin };
        foreach(var cell in MapManager.building.cellBounds.allPositionsWithin)
        {
            var prefab=ExplorationEffects.TilePrefab(MapManager.building.GetTile(cell));
            if(prefab==null) continue;
            var item=prefab.GetComponent<Item_Buliding>(); if(item==null) continue;
            var placed=MapManager.building.GetInstantiatedObject(cell);
            data.buildings.Add(new FieldBuilding { x=cell.x,y=cell.y,item=item.itemSo.idName,
                hp=placed!=null ? placed.GetComponent<Stat>().Hp : prefab.GetComponent<Stat>().maxHP });
        }
        return data;
    }
    public static void Save(string destination = null)
    {
        var data=Capture();
        string target=destination ?? PathName;
        string temp=target+".tmp";
        Directory.CreateDirectory(Application.persistentDataPath);
        File.WriteAllText(temp,JsonConvert.SerializeObject(data));
        if(File.Exists(target)) File.Replace(temp,target,target+".bak"); else File.Move(temp,target);
    }
    public static FieldSave Read(ExplorationContent content, string source = null)
    {
        var data=JsonConvert.DeserializeObject<FieldSave>(File.ReadAllText(source ?? PathName));
        if(data==null || data.version!=1 || data.state==null || !data.state.firstNightCleared || data.slots==null || data.slots.Length!=29 ||
            data.state.research==null || data.state.claims==null || data.state.killedGuards==null || data.buildings==null)
            throw new InvalidDataException("지원하지 않는 탐색 저장 데이터입니다.");
        foreach(var slot in data.slots)
            if(slot==null || slot.count<0 || slot.item!=null && content.Item(slot.item)==null) throw new InvalidDataException("알 수 없는 아이템입니다.");
        foreach(var b in data.buildings)
            if(content.Item(b.item)?.buildTile==null) throw new InvalidDataException("알 수 없는 시설입니다.");
        return data;
    }
    public static IEnumerator Restore(ExplorationContent content, string source = null)
    {
        FieldSave data=null;
        try { data=Read(content,source ?? resumeSource); }
        catch(Exception error) { Debug.LogException(error); }
        FieldSession.ResumeRequested=false; resumeSource=null;
        if(data==null) { FieldSession.NewRun(); yield break; }
        FieldSession.State=data.state;
        ApplyInventory(data.slots,content);
        var game=Managers.Game;
        game.player.transform.position=new Vector2(data.playerX,data.playerY);
        game.tower.transform.position=new Vector2(data.towerX,data.towerY);
        ExplorationEffects.ApplyAll();
        game.player.GetComponent<PlayerStat>().Hp=data.hp; game.player.GetComponent<PlayerStat>().Hunger=data.hunger;
        game.tower.GetComponent<Stat>().Hp=data.towerHp;
        game.timeController.gameObject.SetActive(true);
        game.timeController.RestoreClock(data.day,data.time);
        if (game.timeController.timeType == TimeController.TimeType.Night) game.lightController.SetNight();
        else game.lightController.SetMorning();
        game.canMoveTower=true; game.isCantPlay=false; game.isMovingTower=false;
        Managers.Inven.Coin=data.coin;
        MapManager.building.ClearAllTiles(); MapManager.buildData.Clear();
        foreach(var b in data.buildings) MapManager.building.SetTile(new Vector3Int(b.x,b.y,0),content.Item(b.item).buildTile);
        yield return null; yield return null;
        foreach(var b in data.buildings)
        {
            var go=MapManager.building.GetInstantiatedObject(new Vector3Int(b.x,b.y,0));
            if(go!=null) go.GetComponent<Stat>().Hp=b.hp;
        }
        Managers.Map.SetCanBuildTile(); Managers.Inven.CheckHotBarChoice();
    }
    public static void ApplyInventory(FieldSlot[] slots,ExplorationContent content)
    {
        int i=0;
        foreach(var ui in FieldInventory.Slots)
        {
            var saved=slots[i++];
            if(saved.item==null || saved.count<=0) ui.MakeEmptySlot();
            else Managers.Inven.SetSlot(content.Item(saved.item),ui,saved.count);
        }
    }
    public static IEnumerator RestoreTransfer()
    {
        if(Transfer==null && TransferPreparation==null) yield break;
        while(!FieldInventory.Ready) yield return null;
        var content=Resources.Load<ExplorationContent>("Exploration/Content");
        var preparation=TransferPreparation;
        ApplyInventory(preparation!=null ? preparation.slots : Transfer,content);
        if (preparation!=null)
        {
            FieldSession.State=preparation.state;
            MapManager.building.ClearAllTiles(); MapManager.buildData.Clear();
            foreach(var b in preparation.buildings)
                MapManager.building.SetTile(new Vector3Int(b.x,b.y,0),content.Item(b.item).buildTile);
            // Wait for tile instances and Stat.Start before restoring damage.
            yield return null; yield return null;
            ExplorationEffects.ApplyAll();
            Managers.Game.player.GetComponent<PlayerStat>().Hp=preparation.hp;
            Managers.Game.player.GetComponent<PlayerStat>().Hunger=preparation.hunger;
            Managers.Game.tower.GetComponent<Stat>().Hp=preparation.towerHp;
            foreach(var b in preparation.buildings)
            {
                var go=MapManager.building.GetInstantiatedObject(new Vector3Int(b.x,b.y,0));
                if(go!=null) go.GetComponent<Stat>().Hp=b.hp;
            }
            Managers.Inven.Coin=preparation.coin;
            Managers.Map.SetCanBuildTile();
            Debug.Log($"FIELD_BALANCE: factory transfer preserved {preparation.buildings.Count} facilities and current health");
        }
        Transfer=null; TransferPreparation=null; Managers.Inven.CheckHotBarChoice();
    }
    public static void AddContinueButton(Transform parent,Font font)
    {
        var go=new GameObject("ContinueExploration",typeof(RectTransform)); go.transform.SetParent(parent,false);
        var rect=go.GetComponent<RectTransform>(); rect.anchorMin=rect.anchorMax=new Vector2(1,0);
        rect.pivot=new Vector2(1,0); rect.anchoredPosition=new Vector2(-35,35); rect.sizeDelta=new Vector2(340,65);
        var image=go.AddComponent<UnityEngine.UI.Image>(); image.color=new Color(.08f,.18f,.17f);
        var button=go.AddComponent<UnityEngine.UI.Button>(); button.targetGraphic=image; button.interactable=Exists;
        var label=new GameObject("Label",typeof(RectTransform)); label.transform.SetParent(go.transform,false);
        var lr=label.GetComponent<RectTransform>(); lr.anchorMin=Vector2.zero; lr.anchorMax=Vector2.one; lr.offsetMin=lr.offsetMax=Vector2.zero;
        var text=label.AddComponent<UnityEngine.UI.Text>(); text.font=font; text.fontSize=23; text.alignment=TextAnchor.MiddleCenter;
        text.raycastTarget=false; text.text=Exists ? "야영지 저장 이어하기" : "야영지 저장 없음";
        button.onClick.AddListener(()=>{
            try { RequestResume(); }
            catch(Exception e) { text.text="저장을 읽을 수 없습니다"; Debug.LogException(e); return; }
            UnityEngine.SceneManagement.SceneManager.LoadScene("TutorialScene");
        });
    }
}
