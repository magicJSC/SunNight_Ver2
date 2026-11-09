using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class ExplorationRuntime : MonoBehaviour
{
    public static ExplorationRuntime Instance { get; private set; }
    public ExplorationContent content;
    public static void Create()
    {
        if (Instance != null) return;
        new GameObject("ExplorationRuntime").AddComponent<ExplorationRuntime>();
    }
    IEnumerator Start()
    {
        Instance=this;
        content=Resources.Load<ExplorationContent>("Exploration/Content");
        if (content == null) { Debug.LogError("Exploration content missing. Run Build Exploration Content."); yield break; }
        while (!FieldInventory.Ready) yield return null;
        if (FieldSession.ResumeRequested) yield return FieldCheckpoint.Restore(content);
        MigrateClaims(FieldSession.State,content);
        OuterSalvageLedger.Migrate(FieldSession.State,content);
        FieldHUD.Create(content.font);
        var doorway = new GameObject("ResidentialLaboratoryDoor");
        doorway.transform.SetParent(transform);
        doorway.AddComponent<FieldLaboratory>().Build(this);
        var state = FieldSession.State;
        // Pre-drop checkpoints after the first night already gave the player a map.
        if (state.firstNightCleared && !state.basementMapDropped) state.basementMapReceived = true;
        if (state.basementMapDropped && !state.HasBasementMap)
            CreateMapPickup(new Vector2(state.mapDropX, state.mapDropY));
        foreach(var reward in content.rewards)
        {
            var go=Prop("Reward_"+reward.id,reward.position,
                !string.IsNullOrEmpty(reward.legacySource) && content.salvage!=null ? content.salvage : content.chest,new Color(.95f,.85f,.5f));
            go.AddComponent<FieldReward>().Configure(reward,content);
        }
        foreach(var guard in content.guards)
        {
            if (FieldSession.State.killedGuards.Contains(guard.id)) continue;
            var go=Instantiate(guard.ranged ? content.coala : content.dog,guard.position,Quaternion.identity,transform);
            go.name="Guard_"+guard.id;
            var leash=go.AddComponent<FieldGuard>(); leash.guardId=guard.id; leash.home=guard.position;
            leash.patrolRadius=guard.patrolRadius;
            var monster=go.GetComponent<MonsterController>(); monster.targetType=MonsterController.TargetType.Player;
            monster.getItemList.Clear();
            go.GetComponent<MonsterStat>().maxHP=guard.ranged ? 75 : 30;
        }
        CreateOuterSalvage();
        var camp=Prop("CentralCampWorkbench",new Vector2(58,-69),content.workbench,Color.white);
        camp.AddComponent<FieldWorkbench>();
        ExplorationEffects.ApplyAll();
        Debug.Log($"OPEN_FIELD_READY: {content.rewards.Length} rewards, {content.guards.Length} enemies, {content.guards.Count(x=>x.patrolRadius>0)} patrols");
    }
    void CreateOuterSalvage()
    {
        if(!OuterSalvageLedger.Enabled(content)) return;
        var outer=Object.FindObjectsByType<Transform>().FirstOrDefault(t=>t.name=="OuterField");
        if(outer==null) { Debug.LogError("Outer salvage scenery missing"); return; }
        foreach(var d in content.outerSalvage)
        {
            var scenery=outer.Find(d.sceneryPath)?.GetComponent<SpriteRenderer>();
            if(scenery==null) { Debug.LogError("Outer salvage scenery missing: "+d.id); continue; }
            var go=Prop("Reward_"+d.id,d.position,scenery.sprite,Color.white);
            var visual=go.transform.Find("Visual");
            visual.localScale=scenery.transform.lossyScale;
            visual.localPosition=-(Vector3)Vector2.Scale(scenery.sprite.bounds.center,scenery.transform.lossyScale);
            scenery.enabled=false;
            go.AddComponent<FieldReward>().Configure(d,content);
        }
        Debug.Log($"OUTER_SALVAGE_READY: {content.outerSalvage.Length} searchable debris, {content.outerSalvage.Sum(d=>d.amounts[0])} pieces");
    }
    public static void MigrateClaims(ExplorationState state,ExplorationContent config)
    {
        if(state.layoutVersion>=3) return;
        foreach(var group in config.rewards.Where(r=>!string.IsNullOrEmpty(r.legacySource)).GroupBy(r=>r.legacySource))
        {
            int taken=state.Claimed(group.Key+":0");
            foreach(var piece in group) state.claims.RemoveAll(x=>x.id==piece.id+":0");
            foreach(var piece in group)
            {
                int amount=Mathf.Min(taken,piece.amounts[0]);
                state.AddClaim(piece.id+":0",amount); taken-=amount;
            }
        }
        state.layoutVersion=3;
        Debug.Log("OPEN_FIELD_CLAIMS: migrated v0.2 supply claims without granting inventory");
    }
    internal GameObject Prop(string label,Vector2 position,Sprite sprite,Color color)
    {
        var go=new GameObject(label); go.transform.SetParent(transform); go.transform.position=position;
        var visual=new GameObject("Visual"); visual.transform.SetParent(go.transform,false);
        var renderer=visual.AddComponent<SpriteRenderer>(); renderer.sprite=sprite; renderer.sharedMaterial=content.material;
        renderer.sortingOrder=1; renderer.spriteSortPoint=SpriteSortPoint.Pivot; renderer.color=color;
        visual.transform.localPosition=-sprite.bounds.center;
        var trigger=go.AddComponent<CircleCollider2D>(); trigger.radius=2; trigger.isTrigger=true;
        return go;
    }
    public static void DropBasementMap(GameObject dog)
    {
        var state = FieldSession.State;
        if (Instance == null || state.basementMapDropped || state.HasBasementMap) return;
        var at = dog.transform.position;
        state.basementMapDropped = true;
        state.mapDropX = at.x;
        state.mapDropY = at.y;
        Instance.CreateMapPickup(at);
        FieldHUD.Notify("로봇 잔해에서 접힌 지도가 떨어졌다.");
    }
    void CreateMapPickup(Vector2 at)
    {
        var drop = Prop("DroppedBasementMap", at, Resources.Load<Sprite>("Story/DroppedSurvey"), Color.white);
        drop.AddComponent<LabInteraction>().Configure("떨어진 지도 · F 줍기", () =>
        {
            GrantBasementMap();
            drop.SetActive(false);
            Destroy(drop);
        });
    }
    static void GrantBasementMap()
    {
        if (!FieldSession.Active || FieldSession.State.HasBasementMap) return;
        FieldSession.State.basementMapReceived = true;
        FieldHUD.ShowStory(FieldHUD.StoryPage.Map);
    }
    public static void GrantFirstNightReward()
    {
        bool firstVictory = !FieldSession.State.firstNightCleared;
        FieldSession.State.firstNightCleared=true;
        if (Managers.Game.timeController != null)
        {
            Managers.Game.timeController.RestoreClock(2,330);
            Managers.Game.lightController.SetMorning();
        }
        if (firstVictory) FieldHUD.ShowStory(FieldHUD.StoryPage.Signal);
        if (Instance==null || !FieldInventory.Ready) return;
        int remaining=Mathf.Max(0,4-FieldSession.State.Claimed("N1:0"));
        int given=FieldInventory.Give(Instance.content.scrap,remaining);
        FieldSession.State.AddClaim("N1:0",given);
        FieldHUD.Notify("첫밤 완료! 연구가 열렸습니다. 남은 보급과 약은 야영지에서 받으세요.");
    }
    public bool Cleared(string group) => string.IsNullOrEmpty(group) ||
        content.guards.Where(x=>x.group==group).All(x=>FieldSession.State.killedGuards.Contains(x.id));
    public static string WorkBlockReason(Vector2 station)
    {
        if (SceneManager.GetActiveScene().name!="TutorialScene") return "보스전에서는 연구할 수 없습니다.";
        if (!FieldSession.State.firstNightCleared) return "첫밤의 적을 모두 처치하면 연구가 열립니다.";
        if (!FieldInventory.Ready || Managers.Game.player == null || PlayerController.isDie) return "플레이어 상태를 확인하세요.";
        if (Managers.Game.isMovingTower || Managers.Game.isCantPlay || Managers.Game.tower==null || !Managers.Game.tower.gameObject.activeInHierarchy)
            return "거점을 설치하고 돌아오세요.";
        if (Vector2.Distance(Managers.Game.tower.transform.position,new Vector2(70,-70))>8) return "중앙 야영지 바닥에 거점을 설치하세요.";
        if (Vector2.Distance(Managers.Game.player.transform.position,station)>2.2f) return "작업대 2칸 이내로 이동하세요.";
        foreach(var enemy in Object.FindObjectsByType<MonsterController>())
            if (Vector2.Distance(enemy.transform.position,Managers.Game.player.transform.position)<12 ||
                enemy.target == Managers.Game.player.transform || enemy.target == Managers.Game.tower.transform)
                return "주변 적과 추적 중인 적을 먼저 정리하세요.";
        return null;
    }
    void OnDestroy() { if (Instance==this) Instance=null; }
}
