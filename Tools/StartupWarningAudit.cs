using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

public static class StartupWarningAudit
{
    public static object ScanScriptReferences()
    {
        var missing=new List<object>();
        var cache=new Dictionary<string,string>();
        foreach(var path in AssetDatabase.GetAllAssetPaths().Where(p=>p.StartsWith("Assets/") && (p.EndsWith(".unity") || p.EndsWith(".prefab"))))
        {
            string text=File.ReadAllText(path);
            foreach(System.Text.RegularExpressions.Match match in System.Text.RegularExpressions.Regex.Matches(text,@"m_Script: \{fileID: -?\d+, guid: ([a-f0-9]{32}), type: 3\}"))
            {
                string guid=match.Groups[1].Value;
                if(!cache.TryGetValue(guid,out var script)) {script=AssetDatabase.GUIDToAssetPath(guid);cache[guid]=script;}
                if(string.IsNullOrEmpty(script)) missing.Add(new{path,guid});
            }
        }
        File.WriteAllText("Logs/StartupWarnings/script-references.json",JsonConvert.SerializeObject(missing,Formatting.Indented));
        return missing;
    }
    public static object RemoveVerifiedOrphans()
    {
        var scene=SceneManager.GetActiveScene();
        if(EditorApplication.isPlaying || scene.path!="Assets/2.Scenes/TutorialScene.unity" || scene.isDirty)
            throw new Exception("Expected clean saved TutorialScene in Edit Mode");
        string[] paths={"TutorialGrid/StartHouse/NoSortingWall_Back (1)","TutorialGrid/StartHouse/NoSortingWall_Back (2)"};
        var targets=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).Where(t=>paths.Contains(Hierarchy(t))).ToArray();
        if(targets.Length!=2 || targets.Any(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)!=1))
            throw new Exception("Missing component inventory changed; inspect again");
        int removed=0;
        foreach(var t in targets)
        {
            if(t.GetComponent<VisibleTile_Wall>()==null || t.GetComponent<UnityEngine.Tilemaps.Tilemap>()==null)
                throw new Exception("Required working wall components missing");
            Undo.RegisterCompleteObjectUndo(t.gameObject,"Remove obsolete missing wall component");
            removed+=GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
        }
        EditorSceneManager.MarkSceneDirty(scene);
        if(!EditorSceneManager.SaveScene(scene)) throw new Exception("Scene save failed");
        File.WriteAllText("Logs/StartupWarnings/removal.json",JsonConvert.SerializeObject(new{removed,paths},Formatting.Indented));
        return new{removed,remaining=Inspect()};
    }
    static string Hierarchy(Transform t) => t.parent==null ? t.name : Hierarchy(t.parent)+"/"+t.name;
    public static object Inspect()
    {
        var scenes=new List<object>();
        for(int i=0;i<SceneManager.sceneCount;i++)
        {
            var scene=SceneManager.GetSceneAt(i);
            var missing=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true))
                .Where(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)>0)
                .Select(t=>new{path=Hierarchy(t),count=GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject),
                    source=PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(t.gameObject),id=GlobalObjectId.GetGlobalObjectIdSlow(t.gameObject).ToString()}).ToArray();
            scenes.Add(new{scene.path,scene.name,scene.isDirty,missing});
        }
        Directory.CreateDirectory("Logs/StartupWarnings");
        File.WriteAllText("Logs/StartupWarnings/open-scenes.json",JsonConvert.SerializeObject(scenes,Formatting.Indented));
        return scenes;
    }
}
