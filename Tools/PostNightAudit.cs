using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

// Runs against the live earned checkpoint. No resource, health or combat damage injection.
// Input callbacks are driven programmatically; this is an automated Play Mode traversal, not human play.
public static class PostNightAudit
{
    static string phase;
    static float started,lastWrite,nextPotion;
    static Mouse mouse;
    static Keyboard keyboard;
    static InputAction eat;
    static InputAction talk;
    static HashSet<int> capturedPatterns=new();
    static void CaptureWarning(int pattern)
    {
        if(!capturedPatterns.Add(pattern)) return;
        var camera=Camera.main;
        var rt=new RenderTexture(1280,800,24);
        var previous=RenderTexture.active;
        var texture=new Texture2D(1280,800,TextureFormat.RGB24,false);
        try
        {
            rt.Create();
            UnityEngine.Rendering.RenderPipeline.SubmitRenderRequest(camera,
                new UnityEngine.Rendering.Universal.UniversalRenderPipeline.SingleCameraRequest{destination=rt});
            RenderTexture.active=rt;
            texture.ReadPixels(new Rect(0,0,1280,800),0,0);texture.Apply();
            System.IO.File.WriteAllBytes("Logs/PostNightV03/pattern-"+pattern+".png",texture.EncodeToPNG());
        }
        finally {RenderTexture.active=previous;rt.Release();UnityEngine.Object.Destroy(rt);UnityEngine.Object.Destroy(texture);}
    }
    static PlayerController Player => Managers.Game.player;
    static PlayerStat PStat => Player.GetComponent<PlayerStat>();
    static ItemSO Item(string id) => Resources.Load<ItemSO>("Item/"+id);
    static int Count(string id) => FieldInventory.Count(Item(id));
    static void Move(Vector2 dir) => Player.SendMessage("SetMoveDirection",dir);
    public static object Snapshot() => new {phase,seconds=Time.time-started,scene=SceneManager.GetActiveScene().name,
        player=Player.transform.position.ToString(),hp=PStat.Hp,hunger=PStat.Hunger,
        tower=Managers.Game.tower.transform.position.ToString(),towerHp=Managers.Game.tower.GetComponent<Stat>().Hp,
        inventory=FieldInventory.Slots.Where(x=>x.slotInfo.itemInfo!=null && x.slotInfo.count>0).Select(x=>new{item=x.slotInfo.itemInfo.idName,x.slotInfo.count}).ToArray(),
        research=FieldSession.State.research.ToArray(),kills=FieldSession.State.killedGuards.ToArray(),
        facilities=UnityEngine.Object.FindObjectsByType<Item_Buliding>().Select(x=>new{item=x.itemSo.idName,cell=x.pos.ToString(),hp=x.GetComponent<Stat>().Hp,max=x.GetComponent<Stat>().maxHP}).ToArray(),
        boss=UnityEngine.Object.FindAnyObjectByType<Boss1>()?.GetComponent<Stat>().Hp,
        bossPhase=FieldFoxtonBattle.Active?.Phase,bossPattern=FieldFoxtonBattle.Active?.Pattern,
        enemies=UnityEngine.Object.FindObjectsByType<MonsterController>().Where(x=>Vector2.Distance(x.transform.position,Player.transform.position)<15).Select(x=>new{x.name,hp=x.GetComponent<Stat>().Hp,pos=x.transform.position.ToString()}).ToArray()};
    static void Log()
    {
        System.IO.Directory.CreateDirectory("Logs/PostNightV03");
        System.IO.File.WriteAllText("Logs/PostNightV03/progress.json",JsonConvert.SerializeObject(Snapshot(),Formatting.Indented));
        System.IO.File.AppendAllText("Logs/PostNightV03/samples.jsonl",JsonConvert.SerializeObject(Snapshot())+"\n");
        lastWrite=Time.time;
    }
    static void Mark(string label)
    {
        phase=label;Log();System.IO.File.AppendAllText("Logs/PostNightV03/events.jsonl",JsonConvert.SerializeObject(Snapshot())+"\n");
    }
    static void Save(string label)
    {
        if(SceneManager.GetActiveScene().name=="TutorialScene")
            System.IO.File.WriteAllText("Logs/PostNightV03/"+label+".json",JsonConvert.SerializeObject(FieldCheckpoint.Capture(),Formatting.Indented));
        Mark(label);
    }
    static async Task Wait(float seconds)
    {
        float until=Time.time+seconds;
        while(Time.time<until)
        {
            await Task.Delay(30);
            if(Time.time-lastWrite>2) Log();
            if(System.IO.File.Exists("Logs/PostNightV03/STOP")) throw new Exception("Audit stopped at request file");
            if(PlayerController.isDie || PStat.Hp<=0) throw new Exception("Player died");
            if(Managers.Game.tower.GetComponent<Stat>().Hp<=0) throw new Exception("Tower destroyed");
        }
    }
    static async Task Walk(Vector2 target,float tolerance=.35f)
    {
        float begin=Time.time,lastMoved=Time.time;Vector2 last=Player.transform.position;
        try
        {
            while(Vector2.Distance(Player.transform.position,target)>tolerance)
            {
                Vector2 pos=Player.transform.position;
                if(Vector2.Distance(last,pos)>.12f){last=pos;lastMoved=Time.time;}
                if(Time.time-lastMoved>3 || Time.time-begin>100) throw new Exception("Walk blocked at "+pos+" toward "+target);
                Move((target-pos).normalized);await Wait(.05f);
            }
        }
        finally{Move(Vector2.zero);}
    }
    static async Task Equip(string id)
    {
        int hot=Array.FindIndex(Managers.Inven.hotBarUI.slotList,x=>x.itemUI.slotInfo.itemInfo==Item(id) && x.itemUI.slotInfo.count>0);
        if(hot<0)
        {
            var slot=FieldInventory.Slots.FirstOrDefault(x=>x.slotInfo.itemInfo==Item(id) && x.slotInfo.count>0);
            if(slot==null) throw new Exception("Missing equipment "+id);
            hot=4;Managers.Inven.ChangeItem(slot,Managers.Inven.hotBarUI.slotList[hot].itemUI);
        }
        Managers.Inven.hotBarUI.SendMessage("ChangeChoice",hot);Managers.Inven.CheckHotBarChoice();await Wait(.1f);
    }
    static void Aim(Vector2 world)
    { var p=Camera.main.WorldToScreenPoint(world);InputSystem.QueueStateEvent(mouse,new MouseState{position=new Vector2(p.x,p.y)}); }
    static async Task HealIfNeeded(float threshold=65)
    {
        if(PStat.Hp>threshold || Count("HpPotion")==0 || Time.time<nextPotion) return;
        Move(Vector2.zero);await Equip("HpPotion");int before=Count("HpPotion");float hp=PStat.Hp;
        InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.F13));await Wait(.12f);
        InputSystem.QueueStateEvent(keyboard,new KeyboardState());await Wait(.12f);
        if(Count("HpPotion")!=before-1) throw new Exception("Potion input failed");
        nextPotion=Time.time+10;Mark("potion "+hp+" -> "+PStat.Hp);await Equip("Bat");
    }
    static async Task Fight(string group)
    {
        Mark("fight "+group);await Equip("Bat");float began=Time.time,lastStrike=-1;
        var ids=ExplorationRuntime.Instance.content.guards.Where(x=>x.group==group || x.id==group).Select(x=>x.id).ToArray();
        if(ids.Length==0) throw new Exception("Unknown encounter "+group);
        while(ids.Any(x=>!FieldSession.State.killedGuards.Contains(x)))
        {
            if(Time.time-began>100) throw new Exception("Combat timeout "+group);
            var target=UnityEngine.Object.FindObjectsByType<FieldGuard>().Where(x=>ids.Contains(x.guardId)).OrderBy(x=>Vector2.Distance(Player.transform.position,x.transform.position)).FirstOrDefault();
            if(target==null) {await Wait(.1f);continue;}
            await HealIfNeeded();
            Vector2 delta=target.transform.position-Player.transform.position;
            Aim(target.transform.position);
            Move(delta.magnitude>1.4f?delta.normalized:Vector2.zero);
            if(delta.magnitude<2.8f && Time.time-lastStrike>.65f)
            {if(Managers.Game.weapon!=null) Managers.Game.weapon.SendMessage("OnAttack");lastStrike=Time.time;}
            await Wait(.06f);
        }
        Move(Vector2.zero);Save("after-"+group);
    }
    static async Task Gather(string id)
    {
        Mark("gather "+id);
        var d=ExplorationRuntime.Instance.content.rewards.First(x=>x.id==id);
        await Walk(d.position);
        var reward=GameObject.Find("Reward_"+id).GetComponent<FieldReward>();
        if(reward.Empty) return;
        var interact=Player.GetComponentInChildren<PlayerInteract>();
        interact.SetInteractObj();interact.interactAction?.Invoke();
        await Wait(d.collectSeconds+.3f);interact.cancelAction?.Invoke();
        if(!reward.Empty) throw new Exception("Gather failed "+id);
        Save("pickup-"+id);
    }
    static async Task Research(string id)
    {
        await Walk(new Vector2(58,-69));await Wait(2);
        string reason=ExplorationRuntime.WorkBlockReason(new Vector2(58,-69));if(reason!=null) throw new Exception(reason);
        var interact=Player.GetComponentInChildren<PlayerInteract>();interact.SetInteractObj();interact.interactAction?.Invoke();await Wait(.2f);
        var button=UnityEngine.Object.FindObjectsByType<UnityEngine.UI.Button>().First(x=>x.name==id);
        if(!button.interactable) throw new Exception("Research unavailable "+id);
        button.onClick.Invoke();await Wait(3.3f);
        if(!FieldSession.State.Has(id)) throw new Exception("Research failed "+id);
        UnityEngine.Object.FindObjectsByType<UnityEngine.UI.Button>().First(x=>x.name=="Close").onClick.Invoke();
        Save("research-"+id);
    }
    static async Task Relocate(Vector2 target)
    {
        var before=FieldCheckpoint.Capture();
        Mark("relocate existing tower");
        var controller=Player.GetComponentInChildren<TowerMoveController>();
        if(controller==null) controller=UnityEngine.Object.FindAnyObjectByType<TowerMoveController>();
        controller.SendMessage("ToggleTowerMove");await Wait(.2f);Aim(target);await Wait(.3f);
        controller.SendMessage("ConfirmTowerMove");await Wait(.3f);
        if(Managers.Game.isCantPlay || Vector2.Distance(Managers.Game.tower.transform.position,target)>1) throw new Exception("Tower relocation failed");
        var after=FieldCheckpoint.Capture();
        if(JsonConvert.SerializeObject(before.buildings)!=JsonConvert.SerializeObject(after.buildings) || before.towerHp!=after.towerHp) throw new Exception("Relocation changed facilities/HP");
        Debug.Log("POST_NIGHT_INPUT: mouse relocation preserved facilities and HP");Save("camp-installed");
    }
    static async Task EnterFactory()
    {
        Save("before-factory");
        var door=UnityEngine.Object.FindAnyObjectByType<Boss1EnterTrigger>();
        var panel=door.transform.GetChild(0).gameObject;
        var center=(Vector2)door.GetComponent<Collider2D>().bounds.center;
        float approach=Time.time;
        while(!panel.activeInHierarchy && Time.time-approach<5)
        {Move((center-(Vector2)Player.transform.position).normalized);await Wait(.05f);}
        Move(Vector2.zero);
        if(!panel.activeInHierarchy || Vector2.Distance(Player.transform.position,center)>5)
            throw new Exception("Factory entrance UI is not open near the player");
        var enter=door.GetComponentsInChildren<Transform>(true).First(x=>x.name=="Enter").gameObject;
        enter.GetComponent<UI_EventHandler>().OnPointerClick(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current));
        float begin=Time.time;
        while(SceneManager.GetActiveScene().name!="Boss1" || !FieldInventory.Ready)
        {await Task.Delay(50);if(Time.time-begin>15) throw new Exception("Scene transfer timeout");}
        await Wait(3);Mark("factory transferred");
    }
    static async Task RebuildWalls()
    {
        Mark("rebuild three lost walls using earned scrap");
        Player.SendMessage("OnProduceUI");await Wait(.5f);
        var ui=UnityEngine.Object.FindAnyObjectByType<UI_Produce>();
        foreach(var cell in new[]{new Vector3Int(0,2,0),new Vector3Int(2,-1,0),new Vector3Int(2,0,0)})
        {
            if(MapManager.building.HasTile(cell)) throw new Exception("Existing facility would be overwritten");
            var recipe=ui.GetComponentsInChildren<UI_Produce_Item>().First(x=>x.toMakeItem.toMakeItemSO.idName=="Fence");
            recipe.SetProduce();int before=Count("IronOre");ui.SendMessage("OnProduce");await Wait(.2f);
            if(Count("IronOre")!=before-1) throw new Exception("Wall cost mismatch");
        }
        ui.gameObject.SetActive(false);Managers.Inven.inventoryUI.gameObject.SetActive(false);await Wait(.3f);
        await Equip("Fence");
        foreach(var cell in new[]{new Vector3Int(0,2,0),new Vector3Int(2,-1,0),new Vector3Int(2,0,0)})
        {
            Managers.Map.SetCanBuildTile();await Wait(2);
            Managers.Game.build.BuildAtWorldPoint(MapManager.building.GetCellCenterWorld(cell));await Wait(.3f);
            if(!MapManager.building.HasTile(cell)) throw new Exception("Wall placement failed "+cell);
        }
        Save("walls-rebuilt");
    }
    static async Task BossTrial(float duration,bool testMistakes=false)
    {
        await Equip("Bat");float begin=Time.time,lastStrike=-1,lastBossHp=1100;
        string lastPhase="";float reactAt=0;Vector2 safe=Player.transform.position;
        int shots=0;float minimum=PStat.Hp;
        var testedHits=new HashSet<int>();float beforeMistake=0;
        Vector2 mistakePosition=Player.transform.position;
        while(Time.time-begin<duration)
        {
            var boss=UnityEngine.Object.FindAnyObjectByType<Boss1>(FindObjectsInactive.Include);
            if(boss==null){Mark("boss removed after combat; verify death log");break;}
            if(!boss.gameObject.activeInHierarchy || Managers.Game.isCantPlay)
            {
                Move(Vector2.zero);
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.F14));await Wait(.12f);
                InputSystem.QueueStateEvent(keyboard,new KeyboardState());await Wait(.9f);continue;
            }
            minimum=Mathf.Min(minimum,PStat.Hp);
            float hp=boss.GetComponent<Stat>().Hp;
            if(hp<lastBossHp){shots++;lastBossHp=hp;}
            var activeField=FieldFoxtonBattle.Active;
            if(testMistakes && activeField!=null && activeField.Phase=="Attack" &&
                !testedHits.Contains(activeField.Pattern) && beforeMistake>PStat.Hp)
            {
                testedHits.Add(activeField.Pattern);
                Mark("controlled actual hit pattern="+activeField.Pattern+" damage="+(beforeMistake-PStat.Hp));
            }
            await HealIfNeeded(testMistakes?40:65);
            var monster=UnityEngine.Object.FindObjectsByType<MonsterController>().OrderBy(x=>Vector2.Distance(x.transform.position,Player.transform.position)).FirstOrDefault();
            Vector2 destination;
            var field=FieldFoxtonBattle.Active;
            if(field!=null)
            {
                string cue=field.Phase+field.Pattern;
                if(cue!=lastPhase)
                {
                    lastPhase=cue;reactAt=Time.time+.4f;
                    if(field.Phase=="Telegraph")
                    {
                        CaptureWarning(field.Pattern);
                        Vector2 pos=Player.transform.position;
                        beforeMistake=PStat.Hp;mistakePosition=pos;
                        if(field.Pattern==2)
                        {
                            float[] beams=Enumerable.Range(0,4).Select(i=>-9.5f+i*6+(field.AlternateLanes?3:0)).ToArray();
                            mistakePosition=new Vector2(beams.OrderBy(x=>Mathf.Abs(x-pos.x)).First(),3);
                        }
                        if(field.Pattern==2)
                        {float[] lanes=field.AlternateLanes?new[]{-9.5f,-3.5f,2.5f,8.5f}:new[]{-6.5f,-.5f,5.5f,11.5f};safe=new Vector2(lanes.OrderBy(x=>Mathf.Abs(x-pos.x)).First(),3);}
                        else
                        {
                            float angle=field.BeamAngle*Mathf.Deg2Rad;
                            Vector2 perpendicular=new Vector2(-Mathf.Sin(angle),Mathf.Cos(angle));
                            safe=pos+perpendicular*4;
                            safe=new Vector2(Mathf.Clamp(safe.x,-10,12),Mathf.Clamp(safe.y,-8,9));
                        }
                    }
                }
                destination=field.IsCoreExposed?(Vector2)boss.transform.position+Vector2.up*2.5f:safe;
                if(testMistakes && !testedHits.Contains(field.Pattern) &&
                    (field.Phase=="Telegraph" || field.Phase=="Attack")) destination=mistakePosition;
                if(Time.time<reactAt) destination=Player.transform.position;
            }
            else if(monster!=null) destination=(Vector2)Managers.Game.tower.transform.position+new Vector2(-4,-4);
            else destination=(Vector2)boss.transform.position+Vector2.down*1.5f;
            // Walk around the carried facilities instead of walking into the central tower.
            Vector2 here=Player.transform.position;
            if(destination.y>2 && here.y>-6 && here.y<1.2f && here.x>-3 && here.x<5) destination=new Vector2(-4,here.y);
            else if(destination.y>2 && here.y>-6 && here.y<1.2f) destination=new Vector2(here.x,2.5f);
            Vector2 delta=destination-(Vector2)Player.transform.position;
            Move(delta.magnitude>.4f?delta.normalized:Vector2.zero);
            var target=monster!=null?monster.transform:boss.transform;
            Aim(target.position);
            if(Vector2.Distance(Player.transform.position,target.position)<3 && Time.time-lastStrike>1f)
            {Managers.Game.weapon?.SendMessage("OnAttack");lastStrike=Time.time;}
            await Wait(.06f);
        }
        Move(Vector2.zero);Mark("boss trial ended damage events="+shots+" minimum HP="+minimum);
    }
    public static async Task<object> Run(string commands)
    {
        started=Time.time;nextPotion=0;phase="begin";
        var bg=InputSystem.settings.backgroundBehavior;var ed=InputSystem.settings.editorInputBehaviorInPlayMode;
        InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
        InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        mouse=InputSystem.AddDevice<Mouse>("PostNightMouse");keyboard=InputSystem.AddDevice<Keyboard>("PostNightKeyboard");
        eat=new InputAction("AuditEat",InputActionType.Button,"<Keyboard>/f13");eat.performed+=x=>Player.GetComponentInChildren<EatController>().EatActoin(x);eat.Enable();
        talk=new InputAction("AuditTalk",InputActionType.Button,"<Keyboard>/f14");
        talk.canceled+=x=>{foreach(var dialog in UnityEngine.Object.FindObjectsByType<DialogAnimator>()) dialog.TalkBoxAction(x);};talk.Enable();
        string error=null;
        try
        {
            foreach(var command in commands.Split(';'))
            {
                var a=command.Split(' ');Mark(command);
                if(a[0]=="walk") await Walk(new Vector2(float.Parse(a[1]),float.Parse(a[2])));
                else if(a[0]=="gather") await Gather(a[1]);
                else if(a[0]=="fight") await Fight(a[1]);
                else if(a[0]=="research") await Research(a[1]);
                else if(a[0]=="tower") await Relocate(new Vector2(float.Parse(a[1]),float.Parse(a[2])));
                else if(a[0]=="wait") await Wait(float.Parse(a[1]));
                else if(a[0]=="save") Save(a[1]);
                else if(a[0]=="enter") await EnterFactory();
                else if(a[0]=="boss") await BossTrial(float.Parse(a[1]));
                else if(a[0]=="bossdamage") await BossTrial(float.Parse(a[1]),true);
                else if(a[0]=="walls") await RebuildWalls();
                else if(a[0]=="potion") await HealIfNeeded(80);
                else throw new Exception("Unknown command "+command);
            }
        }
        catch(Exception ex){error=ex.ToString();Mark("ERROR "+ex.Message);}
        finally
        {Move(Vector2.zero);eat.Dispose();talk.Dispose();InputSystem.RemoveDevice(mouse);InputSystem.RemoveDevice(keyboard);InputSystem.settings.backgroundBehavior=bg;InputSystem.settings.editorInputBehaviorInPlayMode=ed;}
        Save("latest-earned");return new{error,state=Snapshot()};
    }
    public static async Task<object> Load(string source)
    {
        FieldCheckpoint.RequestResume(source);
        SceneManager.LoadScene("TutorialScene");
        await Task.Delay(1000);
        for(int i=0;!FieldInventory.Ready || ExplorationRuntime.Instance==null || FieldSession.ResumeRequested;i++)
        {if(i>200) throw new Exception("Restore timeout");await Task.Delay(100);}
        await Task.Delay(300);
        var defense=UnityEngine.Object.FindAnyObjectByType<DefenseEvent>();
        defense.close.SetActive(false);defense.open.SetActive(true);defense.chain.SetActive(false);
        phase="restored "+source;started=Time.time;Mark(phase);return Snapshot();
    }
    public static object InspectBoss()
    {
        var boss=UnityEngine.Object.FindAnyObjectByType<Boss1>();
        var data=new{state=Snapshot(),bossPosition=boss?.transform.position.ToString(),
            directors=UnityEngine.Object.FindObjectsByType<UnityEngine.Playables.PlayableDirector>().Select(x=>new{x.name,x.time,x.duration,state=x.state.ToString()}).ToArray(),
            triggers=UnityEngine.Object.FindObjectsByType<CutSceneTrigger>().Select(x=>new{x.name,pos=x.transform.position.ToString(),bounds=x.GetComponent<Collider2D>()?.bounds.ToString()}).ToArray(),
            lasers=UnityEngine.Object.FindObjectsByType<StrongLaser>(FindObjectsInactive.Include).Select(x=>new{x.name,active=x.gameObject.activeInHierarchy,pos=x.transform.position.ToString()}).ToArray(),
            cannons=UnityEngine.Object.FindObjectsByType<Cannon>().Select(x=>new{pos=x.transform.position.ToString(),targets=x.targets.Where(t=>t!=null).Select(t=>t.name).ToArray()}).ToArray()};
        System.IO.File.WriteAllText("Logs/PostNightV03/boss-inspection.json",JsonConvert.SerializeObject(data,Formatting.Indented));return data;
    }
}
