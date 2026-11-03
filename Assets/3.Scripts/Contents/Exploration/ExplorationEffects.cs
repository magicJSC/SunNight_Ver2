using UnityEngine;

public static class ExplorationEffects
{
    public static bool IsCannon(Stat stat) => stat.GetComponent<Cannon>() != null;
    public static bool IsWall(Stat stat) => stat.GetComponent<Item_Buliding>()?.itemSo?.idName == "Fence";
    public static void Apply(Stat stat)
    {
        if (!FieldSession.Active) return;
        if (IsCannon(stat))
        {
            stat.maxHP = FieldSession.State.Has("U3") ? 30 : 20;
            var build = (BuildStat)stat; build.Damage = FieldSession.State.Has("U2") ? 7 : 5;
            build.attackCool = 1.5f; build.range = 8;
        }
        else if (IsWall(stat)) stat.maxHP = FieldSession.State.Has("U3") ? 45 : 30;
        else if (stat.GetComponent<TowerController>() != null) stat.maxHP = FieldSession.State.Has("U4") ? 650 : 500;
        else if (stat.GetComponent<Boss1>() != null) stat.maxHP = 1100;
    }
    public static void ApplyAll()
    {
        foreach(var stat in Object.FindObjectsByType<Stat>(FindObjectsInactive.Include)) Apply(stat);
    }
    // Paid reinforcement preserves existing damage. Restore uses ApplyAll without awarding HP.
    public static void PurchaseResearch(string id)
    {
        if(FieldSession.State.Has(id)) return;
        var targets=Object.FindObjectsByType<Stat>(FindObjectsInactive.Include);
        var previousMax=new float[targets.Length];
        for(int i=0;i<targets.Length;i++) previousMax[i]=targets[i].maxHP;
        FieldSession.State.research.Add(id);
        ApplyAll();
        if(id!="U3" && id!="U4") return;
        for(int i=0;i<targets.Length;i++)
            if(targets[i].Hp>0 && targets[i].maxHP>previousMax[i])
                targets[i].Hp+=targets[i].maxHP-previousMax[i];
    }
    public static GameObject TilePrefab(UnityEngine.Tilemaps.TileBase tile) => tile is UnityEngine.RuleTile rule ? rule.m_DefaultGameObject : (tile as UnityEngine.Tilemaps.Tile)?.gameObject;
    public static int CannonCount()
    {
        int count = 0;
        var map = MapManager.building;
        if (map != null)
            foreach(var cell in map.cellBounds.allPositionsWithin)
            {
                var prefab = TilePrefab(map.GetTile(cell));
                if (prefab != null && prefab.GetComponent<Cannon>() != null) count++;
            }
        foreach(var cannon in Object.FindObjectsByType<Cannon>(FindObjectsInactive.Include))
            if (map == null || !cannon.transform.IsChildOf(map.transform)) count++;
        return count;
    }
}
