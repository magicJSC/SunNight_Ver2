using UnityEngine;

public class ItemBaker : MonoBehaviour
{
    ItemSO result;
    float remaining;

    public void Bake(ItemSO ingredient)
    {
        result = ingredient.bakeItem;
        remaining = ingredient.bakeTime;
        enabled = true;
    }

    // Inactive packed facilities pause their remaining production time.
    void Update()
    {
        if (!result) return;
        remaining -= Time.deltaTime;
        if (remaining > 0) return;

        var bonfire = GetComponentInParent<BonFireController>();
        bonfire.itemBakers.Add(this);
        bonfire.BakingCount--;
        Managers.Game.GetItem(result, 1, bonfire.transform.position);
        result = null;
        enabled = false;
    }
}
