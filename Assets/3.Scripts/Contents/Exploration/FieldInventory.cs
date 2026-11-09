using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Operations use the same SlotInfo objects as the existing crafting and inventory UI.
public static class FieldInventory
{
    public static bool Ready => Managers.Inven.hotBarUI != null && Managers.Inven.inventoryUI != null &&
        Managers.Inven.hotBarUI.slotList != null && Managers.Inven.inventoryUI.slotList != null &&
        Managers.Inven.hotBarUI.slotList.All(x => x != null && x.itemUI != null && x.itemUI.slotInfo != null) &&
        Managers.Inven.inventoryUI.slotList.All(x => x != null && x.itemUI != null && x.itemUI.slotInfo != null);
    public static IEnumerable<UI_Item> Slots => Managers.Inven.hotBarUI.slotList.Take(InvenManager.BaseSlotIndex).Select(x => x.itemUI)
        .Concat(Managers.Inven.inventoryUI.slotList.Select(x => x.itemUI));
    public static void RestoreLegacyStack(InvenManager.SlotInfo legacy)
    {
        if (legacy == null || legacy.itemInfo == null || !Ready) return;
        legacy.count -= Give(legacy.itemInfo, legacy.count);
        if (legacy.count == 0)
        {
            legacy.itemInfo = null;
            legacy.keyType = Define.KeyType.Empty;
        }
    }
    public static int Count(ItemSO item) => Ready ? Slots.Where(x => x.slotInfo.itemInfo == item).Sum(x => x.slotInfo.count) : 0;
    // Returns accepted count. A full inventory never consumes the unaccepted reward.
    public static int Give(ItemSO item, int amount)
    {
        if (!Ready || item == null || amount <= 0) return 0;
        int remaining = amount;
        foreach (var slot in Slots.OrderBy(x => x.slotInfo.itemInfo == item ? 0 : 1))
        {
            var info = slot.slotInfo;
            if (info.itemInfo != null && info.itemInfo != item) continue;
            int room = Mathf.Max(0, Mathf.Max(1,item.maxAmount) - (info.itemInfo == item ? info.count : 0));
            int accepted = Mathf.Min(room,remaining);
            if (accepted == 0) continue;
            bool equip = info.itemInfo == null && item.itemType == Define.ItemType.Tool &&
                slot == Managers.Inven.hotBarUI.slotList[Managers.Inven.choiceIndex].itemUI;
            Managers.Inven.SetSlot(item, slot, (info.itemInfo == item ? info.count : 0) + accepted);
            if (equip) Managers.Inven.CheckHotBarChoice();
            remaining -= accepted;
            if (remaining == 0) break;
        }
        return amount - remaining;
    }
    public static bool Spend(ItemSO scrap, int scrapCost, ItemSO parts, int partsCost)
    {
        if (!Ready || Count(scrap)<scrapCost || Count(parts)<partsCost) return false;
        Remove(scrap,scrapCost); Remove(parts,partsCost); return true;
    }
    static void Remove(ItemSO item, int amount)
    {
        foreach(var slot in Slots)
        {
            if (slot.slotInfo.itemInfo != item || amount <= 0) continue;
            int used = Mathf.Min(amount,slot.slotInfo.count);
            slot.slotInfo.count -= used; amount -= used;
            slot.SetInfo();
        }
    }
}
