using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
public static class ExplorationResumeSmoke
{
    public static async Task<string> Run()
    {
        string path=System.IO.Path.GetFullPath("Logs/exploration-test-save.json");
        FieldCheckpoint.RequestResume(path);
        SceneManager.LoadScene("TutorialScene");
        float start=Time.realtimeSinceStartup;
        do {
            await Task.Delay(100);
            if(Time.realtimeSinceStartup-start>20) throw new Exception("Resume initialization exceeded 20 seconds");
        } while(ExplorationRuntime.Instance==null || !FieldInventory.Ready || UnityEngine.Object.FindObjectsByType<FieldReward>().Length!=21 || FieldSession.ResumeRequested);
        await Task.Delay(300);
        var c=ExplorationRuntime.Instance.content;
        if(FieldInventory.Count(c.scrap)!=7 || FieldInventory.Count(c.parts)!=2 || !FieldSession.State.Has("U3")) throw new Exception("Resume inventory/research mismatch");
        if(!GameObject.Find("Reward_A1").GetComponent<FieldReward>().Empty) throw new Exception("Chest respawned its reward");
        if(UnityEngine.Object.FindObjectsByType<FieldGuard>().Any(x=>x.guardId=="D1a")) throw new Exception("Dead guard respawned");
        if(ExplorationEffects.CannonCount()!=3) throw new Exception("Saved cannons were not restored");
        var gun=MapManager.building.GetInstantiatedObject(new Vector3Int(1,0,0)).GetComponent<BuildStat>();
        if(gun.Hp!=12 || gun.maxHP!=30) throw new Exception("Facility HP changed during fresh scene load");
        int nights=0; Managers.Game.timeController.nightEvent+=()=>nights++;
        Managers.Game.timeController.RestoreClock(2,1350);
        if(nights!=0) throw new Exception("Restoring a cleared night restarted its wave");
        var station=UnityEngine.Object.FindAnyObjectByType<FieldWorkbench>();
        Managers.Game.player.transform.position=station.transform.position;
        FieldHUD.Open(station);
        var canvas=UnityEngine.Object.FindAnyObjectByType<FieldHUD>().GetComponent<Canvas>();
        canvas.renderMode=RenderMode.ScreenSpaceCamera; canvas.worldCamera=Camera.main; canvas.planeDistance=1;
        const string result="PASS: fresh TutorialScene resume restores inventory/research/buildings/depleted chests/dead guards; cleared-night load does not restart wave";
        System.IO.File.WriteAllText("Logs/exploration-resume-smoke.txt",result); Debug.Log("EXPLORATION_TEST: "+result); return result;
    }
}
