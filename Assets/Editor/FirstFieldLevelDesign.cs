using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

/// <summary>Editor authoring only: the shipped field consists entirely of static renderers.</summary>
public static class FirstFieldLevelDesign
{
    public const string PrefabPath = "Assets/LevelDesign/FirstField/FirstFieldDressing.prefab";
    public const string ScenePath = "Assets/2.Scenes/TutorialScene.unity";
    const string Art = "Assets/1.Asset/Sprite/Tile/PAL_SET1/";
    const string Deco = "Assets/Resources/Prefabs/Map/Deco/";
    const string TileFolder = "Assets/LevelDesign/FirstField/Tiles";
    const long InstanceId = 780026100000000001;

    // Exterior anchors B=(-4,-150), D=(156,-160) avoid the existing Event1/2 buildings.
    // Traversal probes only: these historical paths are no longer painted or reserved corridors.
    static Vector2[] Points(params float[] xy) => Enumerable.Range(0, xy.Length / 2)
        .Select(i => new Vector2(xy[i * 2], xy[i * 2 + 1])).ToArray();
    static readonly Vector2[][] Routes = {
        Points(-33,-40, -28,-58, -4,-64, 32,-74, 70,-70),
        Points(-28,-58, -60,-70, -80,-110, -80,-185, -4,-185, -4,-150),
        Points(-4,-64, 10,-98, -4,-120, -4,-150),
        Points(70,-70, 95,-102, 112,-125, 112,-160, 156,-160),
        Points(156,-160, 70,-185, -4,-185, -4,-150),
        Points(70,-70, 90,-20, 125,20),
        Points(125,20, 195,-5, 205,-100, 205,-160, 156,-160),
        Points(156,-160, 205,-160, 208,-116, 224,-70),
        Points(125,20, 212,16, 224,-70),
        Points(224,-70, 256.53f,-57, 256.53f,-48)
    };
    static readonly string[] TrailNames = { "Exit_A_H", "J_C_B", "A_B", "H_D", "D_B", "H_E", "E_D", "D_F", "E_F", "F_X" };
    static readonly Vector2[][] Candidates = {
        new [] { new Vector2(-98,32), new Vector2(-98,0), new Vector2(-98,-28), new Vector2(-72,52), new Vector2(-37,52), new Vector2(-11,13) },
        Points(-39,-59, -15,-55, 4,-75, 22,-61, 24,-42, 42,-29, 58,-9, 27,-99),
        Points(-7,-135, 6,-155, -18,-184, -44,-185, -52,-154, 24,-143, 40,-174, 50,-121, 59,-158),
        Points(-70,-95, -92,-108, -69,-125, -93,-143, -68,-165, -102,-80, -83,-172, -61,-84),
        Points(54,-70, 70,-59, 86,-70, 55,-82, 70,-83, 87,-83),
        Points(115,-142, 135,-155, 158,-150, 179,-153, 186,-168, 140,-173, 99,-125, 118,-185, 175,-185, 207,-149),
        Points(113,29, 129,31, 141,12, 115,8, 107,-3, 96,42, 151,40, 175,18, 76,10),
        Points(214,-70, 237,-76, 247,-68, 267,-60),
        Points(23,-83, 53,-62, 94,-42, 105,-82, 62,-190, 193,-65, 213,-137,
            -50,-89, -100,-58, -46,-130, -57,-174, 35,-13, 51,27, 18,-190,
            73,-139, 80,-184, 126,-87, 177,-90, 197,-34, 218,10, 208,-110, 223,-167)
    };
    static readonly string[] ZoneNames = { "T_TutorialEdge", "A_ExitSalvage", "B_Housing", "C_Forest", "H_CentralCamp", "D_Scrapyard", "E_Workshop", "F_FactoryOutskirts", "Links" };
    static readonly Rect[] Protected = {
        Rect.MinMaxRect(-90,-40,-19,44), Rect.MinMaxRect(-32,-174,-14,-125),
        Rect.MinMaxRect(129,-146,182,-105), Rect.MinMaxRect(230,-47,282,-14)
    };
    // Include both the draft's centred space and the current grid's y=-5..+6 cells.
    // A 20x14 envelope also leaves a one-unit vertical margin for the offset grid.
    static readonly Rect[] Clearings = {
        new Rect(60,-77,20,14), new Rect(146,-167,20,14), new Rect(115,13,20,14)
    };
    static readonly Vector2[] Offsets = { Vector2.zero, new Vector2(-2.05f,-.5f), new Vector2(1.8f,-.7f), new Vector2(.7f,-1.8f), new Vector2(-1.1f,1.1f) };
    static Material material;
    static readonly Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();

    [MenuItem("SunNight/Level Design/Rebuild First Field Dressing")]
    public static void Build()
    {
        GameObject root = null;
        var exploration=Resources.Load<ExplorationContent>("Exploration/Content");
        var previousSalvage=exploration.outerSalvage;
        bool savedPrefab=false;
        var previewScene = EditorSceneManager.NewPreviewScene();
        try
        {
            EnsureFolder("Assets", "LevelDesign");
            EnsureFolder("Assets/LevelDesign", "FirstField");
            EnsureFolder("Assets/LevelDesign/FirstField", "Tiles");
            Sprites.Clear();
            material = AssetDatabase.LoadAssetAtPath<GameObject>(Deco + "StreetLamp.prefab").GetComponent<SpriteRenderer>().sharedMaterial;
            if (material == null) throw new InvalidOperationException("Existing Sprite-Lit material is missing.");
            root = new GameObject("FirstFieldDressing");
            SceneManager.MoveGameObjectToScene(root, previewScene);
            var placement = new StringBuilder("zone,cluster,candidate_x,candidate_y,placed_x,placed_y,props\n");
            for (int zone = 0; zone < Candidates.Length; zone++)
            {
                var zoneRoot = Child(root.transform, ZoneNames[zone]);
                for (int index = 0; index < Candidates[zone].Length; index++)
                {
                    var candidate = Candidates[zone][index];
                    var group = Child(zoneRoot, "Cluster_" + (index + 1).ToString("00"));
                    group.position = candidate;
                    Populate(group, zone, index);
                    AlignToVisibleVerge(group, zone, candidate);
                    var p = group.position;
                    placement.AppendFormat(System.Globalization.CultureInfo.InvariantCulture,
                        "{0},{1},{2},{3},{4:F2},{5:F2},{6}\n", ZoneNames[zone], index + 1, candidate.x, candidate.y, p.x, p.y, group.childCount);
                }
            }
            AddGround(root.transform);
            OuterFieldDressing.Apply(root, OuterFieldDressing.Ground());
            Validate(root);
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath,out savedPrefab);
            if(!savedPrefab) throw new InvalidOperationException("First field prefab could not be saved.");
            AssetDatabase.SaveAssets();
            AttachToScene();
            Directory.CreateDirectory("Logs/FirstFieldPreview");
            File.WriteAllText("Logs/FirstFieldPreview/placements.csv", placement.ToString(), new UTF8Encoding(false));
            Debug.Log("FIRST_FIELD_BUILD_OK: v0.3 open field; isolated landmarks replace connected trails.");
        }
        catch
        {
            if(!savedPrefab) { exploration.outerSalvage=previousSalvage;EditorUtility.SetDirty(exploration); }
            throw;
        }
        finally
        {
            if (root != null) Object.DestroyImmediate(root);
            EditorSceneManager.ClosePreviewScene(previewScene);
        }
    }

    public static void BuildBatch()
    {
        try { Build(); EditorApplication.Exit(0); }
        catch (Exception error) { Debug.LogException(error); EditorApplication.Exit(1); }
    }

    public static void BuildAndCaptureBatch()
    {
        try { Build(); FirstFieldLevelDesignPreview.Capture(); }
        catch (Exception error) { Debug.LogException(error); EditorApplication.Exit(1); }
    }

    static Transform Child(Transform parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.transform;
    }

    static void EnsureFolder(string parent, string name)
    {
        if (!AssetDatabase.IsValidFolder(parent + "/" + name)) AssetDatabase.CreateFolder(parent, name);
    }

    static Sprite SpriteAt(string path, string name)
    {
        string key = path + ":" + name;
        if (!Sprites.TryGetValue(key, out var sprite))
        {
            sprite = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault(s => s.name == name);
            if (sprite == null) throw new InvalidOperationException("Missing sprite: " + key);
            Sprites.Add(key, sprite);
        }
        return sprite;
    }

    static void Populate(Transform group, int zone, int index)
    {
        int count = zone == 0 ? 4 : 5;
        for (int slot = 0; slot < count; slot++)
        {
            Sprite sprite;
            string label;
            float scale = .85f + ((index + slot) % 3) * .075f;
            bool tree = (zone == 0 && slot < 2) || (zone == 3 && slot < 3) || (zone == 8 && slot == 0);
            if (tree)
            {
                int variant = (index + slot) % 3 + 1;
                label = "Tree" + variant;
                sprite = SpriteAt(Art + "Animated/tree" + variant + ".png", "tree" + variant + "_0");
                scale = slot == 1 ? .20f : .28f;
            }
            else
            {
                string[][] palettes = {
                    new [] { "Tire2", "DrumCan", "Scrap37", "Scrap35", "Box" },
                    new [] { "Sign", "Tire2", "Scrap37", "DrumCan", "Scrap35" },
                    new [] { "StreetLamp", "Sofa", "Box", "Chair1", "TrashCan" },
                    new [] { "Firestone", "Box", "Tire", "Scrap37", "DrumCan" },
                    new [] { "StreetLamp", "Desk", "Chair1", "Box", "Firestone" },
                    new [] { "Scrap32", "Tire", "DrumCan5", "Scrap37", "Scrap51" },
                    new [] { "Desk2", "Locker", "DrumCan", "Box2", "Tire" },
                    new [] { "StreetLamp2", "Box", "DrumCan", "Tire2", "Scrap35" },
                    new [] { "Sign", "Tire", "Box", "Scrap37", "DrumCan" }
                };
                label = palettes[zone][slot];
                if (zone == 1 && slot == 0 && index != 1) label = index == 2 ? "StreetLamp2" : "Scrap35";
                if (label.StartsWith("Scrap", StringComparison.Ordinal)) sprite = SpriteAt(Art + "dec_props.png", "dec_props_" + label.Substring(5));
                else
                {
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Deco + label + ".prefab");
                    if (prefab == null) throw new InvalidOperationException("Missing prop " + label);
                    sprite = prefab.GetComponent<SpriteRenderer>().sprite;
                }
            }
            var prop = Child(group, (slot + 1).ToString("00") + "_" + label);
            prop.localScale = Vector3.one * scale;
            // Position by visible centre instead of assuming the imported pivots are identical.
            var offset = Offsets[slot];
            if (index % 2 == 1) offset.x = -offset.x;
            prop.localPosition = new Vector3(offset.x - sprite.bounds.center.x * scale, offset.y - sprite.bounds.center.y * scale, 0);
            var renderer = prop.gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sharedMaterial = material;
            renderer.sortingLayerID = 0;
            renderer.sortingOrder = 1;
            renderer.spriteSortPoint = SpriteSortPoint.Pivot;
        }
    }

    static Bounds GroupBounds(Transform group)
    {
        var renderers = group.GetComponentsInChildren<SpriteRenderer>();
        var bounds = renderers[0].bounds;
        foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
        return bounds;
    }

    static Rect BoundsRect(Bounds bounds) => Rect.MinMaxRect(bounds.min.x, bounds.min.y, bounds.max.x, bounds.max.y);

    static void AlignToVisibleVerge(Transform group, int zone, Vector2 candidate)
    {
        // Search near each art-directed candidate; never slide a cluster across the map.
        for (int attempt = 0; attempt < 321; attempt++)
        {
            float radius = attempt == 0 ? 0 : 1 + (attempt - 1) / 16;
            float angle = (attempt % 16) * Mathf.PI / 8;
            group.position = candidate + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            if (group.GetComponentsInChildren<SpriteRenderer>().All(r => IsClear(BoundsRect(r.bounds)))) return;
        }
        throw new InvalidOperationException("Could not place cluster safely: " + group.parent.name + "/" + group.name);
    }

    static bool IsClear(Rect rect)
    {
        if (Protected.Any(r => r.Overlaps(rect)) || Clearings.Any(r => r.Overlaps(rect))) return false;
        // Keep pickups visible from every approach, instead of lining art up along a trail.
        var content=Resources.Load<ExplorationContent>("Exploration/Content");
        if(content!=null && content.rewards.Any(r=>rect.Overlaps(new Rect(r.position-Vector2.one*2,Vector2.one*4)))) return false;
        return true;
    }

    static bool SegmentCrossesRect(Vector2 start, Vector2 end, Rect rect)
    {
        Vector2 delta = end - start;
        float near = 0, far = 1;
        for (int axis = 0; axis < 2; axis++)
        {
            float lo = axis == 0 ? rect.xMin : rect.yMin;
            float hi = axis == 0 ? rect.xMax : rect.yMax;
            if (Mathf.Abs(delta[axis]) < .0001f)
            {
                if (start[axis] < lo || start[axis] > hi) return false;
                continue;
            }
            float a = (lo - start[axis]) / delta[axis], b = (hi - start[axis]) / delta[axis];
            near = Mathf.Max(near, Mathf.Min(a, b));
            far = Mathf.Min(far, Mathf.Max(a, b));
            if (near > far) return false;
        }
        return true;
    }

    static Tile GroundTile(int spriteNumber)
    {
        string path = TileFolder + "/Ground_" + spriteNumber + ".asset";
        var tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
        bool isNew = tile == null;
        if (isNew) tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = SpriteAt(Art + "Sliced/main_lev.png", "main_lev_" + spriteNumber);
        tile.colliderType = Tile.ColliderType.None;
        tile.flags = TileFlags.None;
        tile.color = Color.white;
        if (isNew) AssetDatabase.CreateAsset(tile, path);
        else EditorUtility.SetDirty(tile);
        return tile;
    }

    static Vector2 AlongRoute(Vector2[] route, float distance, out Vector2 direction)
    {
        for (int i = 1; i < route.Length; i++)
        {
            float length = Vector2.Distance(route[i - 1], route[i]);
            direction = (route[i] - route[i - 1]).normalized;
            if (distance <= length) return route[i - 1] + direction * distance;
            distance -= length;
        }
        direction = Vector2.up;
        return route[route.Length - 1];
    }

    static void AddGround(Transform parent)
    {
        var grid = Child(parent, "GroundTraces");
        grid.gameObject.AddComponent<Grid>();
        var tiles = new Dictionary<int, Tile>();
        var allOccupied = new HashSet<Vector3Int>();
        foreach (int id in new [] { 15,16,18,20,21,213,216,217,218,219,416,417,419,421,422 }) tiles[id] = GroundTile(id);
        for (int layer = 0; layer < 2; layer++)
        {
            var go = Child(grid, layer==0 ? "CampAndLandmarkPatches" : "BrokenRoadRemnants");
            var map = go.gameObject.AddComponent<Tilemap>();
            map.color = new Color(1,1,1,.60f);
            var renderer = go.gameObject.AddComponent<TilemapRenderer>();
            renderer.sharedMaterial = material;
            renderer.sortingLayerID = 0;
            renderer.sortingOrder = -9;
            renderer.mode = TilemapRenderer.Mode.Chunk;
            var occupied = new HashSet<Vector3Int>();
            // A broad, empty dirt pad reads as the camp's installation area at gameplay zoom.
            if (layer == 0)
            {
                for (int x = 60; x < 80; x++)
                for (int y = -77; y < -63; y++) occupied.Add(new Vector3Int(x, y, 0));
                // Small irregular islands around points of interest; no connecting strokes.
                foreach(var centre in Candidates.Skip(1).SelectMany(x=>x))
                for(int dx=-3;dx<=3;dx++)
                for(int dy=-2;dy<=2;dy++)
                {
                    if(dx*dx/9f+dy*dy/4f>1 || (dx*7+dy*11+(int)centre.x)%7==0) continue;
                    var cell=new Vector3Int(Mathf.RoundToInt(centre.x)+dx,Mathf.RoundToInt(centre.y)+dy,0);
                    if(!Protected.Any(r=>r.Overlaps(new Rect(cell.x,cell.y,1,1))) && !Clearings.Any(r=>r.Contains(new Vector2(cell.x,cell.y)))) occupied.Add(cell);
                }
            }
            else
            {
                foreach(var remnant in new[]{Points(-8,-155,-2,-155),Points(150,-176,157,-176),Points(245,-57,252,-57)})
                {
                    float length=Vector2.Distance(remnant[0],remnant[1]);
                    for(float distance=0;distance<=length;distance+=.5f)
                    for(int lane=0;lane<2;lane++)
                    {
                        Vector2 p=AlongRoute(remnant,distance,out _)+Vector2.up*lane;
                        occupied.Add(new Vector3Int(Mathf.FloorToInt(p.x),Mathf.FloorToInt(p.y),0));
                    }
                }
            }
            foreach (var cell in occupied)
            {
                if (!allOccupied.Add(cell)) continue;
                bool top = !occupied.Contains(cell + Vector3Int.up), bottom = !occupied.Contains(cell + Vector3Int.down);
                bool left = !occupied.Contains(cell + Vector3Int.left), right = !occupied.Contains(cell + Vector3Int.right);
                int number = top ? (left ? 15 : right ? 21 : new [] {16,18,20}[(cell.x & int.MaxValue)%3])
                    : bottom ? (left ? 416 : right ? 422 : new [] {417,419,421}[(cell.x & int.MaxValue)%3])
                    : left ? 213 : right ? 219 : 216 + ((cell.x + cell.y) & int.MaxValue)%3;
                map.SetTile(cell, tiles[number]);
            }
            map.CompressBounds();
        }
    }

    public static void Validate(GameObject root)
    {
        foreach (var component in root.GetComponentsInChildren<Component>(true))
            if (!(component is Transform || component is SpriteRenderer || component is Grid || component is Tilemap || component is TilemapRenderer))
                throw new InvalidOperationException("Unexpected component: " + (component == null ? "missing script" : component.GetType().Name));
        var outer = root.transform.Find(OuterFieldDressing.RootName);
        var props = root.GetComponentsInChildren<SpriteRenderer>(true).Where(r => outer == null || !r.transform.IsChildOf(outer)).ToArray();
        int expected = Candidates.Select((points, zone) => points.Length * (zone == 0 ? 4 : 5)).Sum();
        if (props.Length != expected) throw new InvalidOperationException("Expected " + expected + " props, got " + props.Length);
        foreach (var prop in props)
        {
            if (prop.sprite == null || prop.sharedMaterial == null || prop.sortingLayerID != 0 || prop.sortingOrder != 1)
                throw new InvalidOperationException("Invalid renderer: " + prop.name);
            if (!IsClear(BoundsRect(prop.bounds))) throw new InvalidOperationException("Protected space overlaps: " + prop.transform.parent.name + "/" + prop.name);
        }
        var maps = root.GetComponentsInChildren<Tilemap>(true);
        if (maps.Length != 2) throw new InvalidOperationException("Expected patches and broken road remnants.");
        int cells = 0;
        foreach (var map in maps)
        foreach (var pos in map.cellBounds.allPositionsWithin)
        {
            if (!map.HasTile(pos)) continue;
            if (map.GetColliderType(pos) != Tile.ColliderType.None) throw new InvalidOperationException("Ground tile has collision.");
            cells++;
        }
        if (cells < 350 || cells>2600) throw new InvalidOperationException("Unexpected ground patch coverage: " + cells);
        Debug.Log("FIRST_FIELD_VALIDATION_OK: props=" + props.Length + ", groundCells=" + cells + "; open field, protected regions and building clearings preserved.");
    }

    public static void AuditSceneRoutes()
    {
        foreach (var map in Object.FindObjectsByType<TilemapCollider2D>(FindObjectsSortMode.None))
            map.ProcessTilemapChanges();
        Physics2D.SyncTransforms();
        var rows = new StringBuilder("trail,x,y,collider,layer\n");
        int samples = 0, blocked = 0;
        for (int trail = 0; trail < Routes.Length; trail++)
        {
            var route = Routes[trail];
            float length = Enumerable.Range(1, route.Length - 1).Sum(i => Vector2.Distance(route[i-1], route[i]));
            for (float distance = 2; distance < length - 1; distance += 1)
            {
                var point = AlongRoute(route, distance, out _);
                samples++;
                foreach (var hit in Physics2D.OverlapCircleAll(point, .45f))
                {
                    if (hit.isTrigger || !hit.enabled || !hit.gameObject.activeInHierarchy) continue;
                    if (hit.attachedRigidbody != null && hit.attachedRigidbody.bodyType != RigidbodyType2D.Static) continue;
                    blocked++;
                    rows.AppendFormat(System.Globalization.CultureInfo.InvariantCulture,
                        "{0},{1:F2},{2:F2},{3},{4}\n", TrailNames[trail], point.x, point.y,
                        hit.name.Replace(',', '_'), LayerMask.LayerToName(hit.gameObject.layer));
                }
            }
        }
        File.WriteAllText("Logs/FirstFieldPreview/route-collisions.csv", rows.ToString());
        Debug.Log("FIRST_FIELD_ROUTE_AUDIT: samples=" + samples + ", staticColliderHits=" + blocked);
        if (blocked > 0) throw new InvalidOperationException("Exploration trail overlaps static colliders; see route-collisions.csv");
    }

    static void AttachToScene()
    {
        // Saving the 76 MB scene through the editor would rewrite unrelated serialized tile data.
        // Append one native prefab instance instead, preserving every existing serialized object.
        string guid = AssetDatabase.AssetPathToGUID(PrefabPath);
        string text = File.ReadAllText(ScenePath);
        if (text.Contains("guid: " + guid)) return;
        if (text.Contains("&" + InstanceId)) throw new InvalidOperationException("Reserved prefab instance ID already exists.");
        string marker = "--- !u!1660057539 &9223372036854775807";
        int insertion = text.LastIndexOf(marker, StringComparison.Ordinal);
        if (insertion < 0) throw new InvalidOperationException("SceneRoots was not found; scene left untouched.");
        string nl = text.Contains("\r\n") ? "\r\n" : "\n";
        string block = string.Join(nl, new [] {
            "--- !u!1001 &" + InstanceId, "PrefabInstance:", "  m_ObjectHideFlags: 0", "  serializedVersion: 2",
            "  m_Modification:", "    serializedVersion: 3", "    m_TransformParent: {fileID: 0}",
            "    m_Modifications: []", "    m_RemovedComponents: []", "    m_RemovedGameObjects: []",
            "    m_AddedGameObjects: []", "    m_AddedComponents: []",
            "  m_SourcePrefab: {fileID: 100100000, guid: " + guid + ", type: 3}", ""
        });
        text = text.Insert(insertion, block);
        text += (text.EndsWith(nl, StringComparison.Ordinal) ? "" : nl) + "  - {fileID: " + InstanceId + "}" + nl;
        File.WriteAllText(ScenePath, text, new UTF8Encoding(false));
        AssetDatabase.ImportAsset(ScenePath);
    }
}
