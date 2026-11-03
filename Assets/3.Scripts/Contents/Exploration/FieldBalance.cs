using UnityEngine;
using UnityEngine.SceneManagement;

// The legacy tutorial remains playable, but must not duplicate the v0.2 regional budget.
public static class FieldBalance
{
    public static bool HungerActive => FieldSession.Active && !FieldSession.State.firstNightCleared &&
        Managers.Game.timeController != null && Managers.Game.timeController.gameObject.activeInHierarchy &&
        !Managers.Game.isCantPlay && !PlayerController.isDie;
    public static string Configure(TrashPileController pile)
    {
        if (!FieldSession.Active || SceneManager.GetActiveScene().name != "TutorialScene") return null;
        var p = pile.transform.position;
        string item = null;
        int count = 0;
        if (Vector2.Distance(p, new Vector2(-68.67f, 13.82f)) < .5f) { item = "IronOre"; count = 4; }
        else if (Vector2.Distance(p, new Vector2(-59.41f, -22.01f)) < .5f) { item = "Bat"; count = 1; }
        else if (Vector2.Distance(p, new Vector2(-44.51f, -21.60f)) < .5f) { item = "Steak"; count = 1; }
        else if (Vector2.Distance(p, new Vector2(-35.63f, -29.43f)) < .5f ||
                 Vector2.Distance(p, new Vector2(-23.84f, -23.29f)) < .5f ||
                 Vector2.Distance(p, new Vector2(-33.64f, -12.45f)) < .5f) { item = "IronOre"; count = 1; }
        else if (!(p.x > -32 && p.x < -14 && p.y > -132 && p.y < -130)) return null;
        pile.getItemList.Clear();
        if (item != null)
            pile.getItemList.Add(new TrashPileController.GetItem { item = Resources.Load<ItemSO>("Item/" + item), count = count, probability = 100 });
        Debug.Log($"FIELD_BALANCE: {pile.name} {item ?? "depleted"} x{count}");
        return "tutorial:" + pile.name;
    }
}
