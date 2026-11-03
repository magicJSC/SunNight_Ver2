using System;
using UnityEngine;

[CreateAssetMenu(menuName="SunNight/Exploration Content")]
public sealed class ExplorationContent : ScriptableObject
{
    public ItemSO scrap, parts, food, potion;
    public Sprite chest, workbench, salvage;
    public Material material;
    public Font font;
    public GameObject dog, coala;
    public FieldRewardDefinition[] rewards;
    public FieldGuardDefinition[] guards;
    public FieldRewardDefinition[] outerSalvage = new FieldRewardDefinition[0];
    public ItemSO Item(string id)
    {
        switch(id) { case "IronOre": return scrap; case "MaintenancePart": return parts;
            case "Steak": return food; case "HpPotion": return potion; default: return Resources.Load<ItemSO>("Item/"+id); }
    }
}
[Serializable] public sealed class FieldRewardDefinition
{
    public string id, region, group;
    public Vector2 position;
    public string[] items;
    public int[] amounts;
    public bool requiresFirstNight;
    public string legacySource;
    public bool salvageFragments;
    public string sceneryPath;
    public float collectSeconds = 4;
}
[Serializable] public sealed class FieldGuardDefinition
{
    public string id, group;
    public Vector2 position;
    public bool ranged;
    public float patrolRadius;
}
