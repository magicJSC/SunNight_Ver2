using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class BaseSlotAudit
{
    static void Check(bool ok, string name)
    {
        if (!ok) throw new InvalidOperationException(name);
    }

    public static string Run()
    {
        Check(Application.isPlaying && FieldInventory.Ready, "Play Mode with inventory required");
        var content = Resources.Load<ExplorationContent>("Exploration/Content");
        var original = FieldCheckpoint.CaptureInventory();
        var baseSlot = Managers.Inven.hotBarUI.slotList[InvenManager.BaseSlotIndex];
        try
        {
            var old = Enumerable.Range(0, 29).Select(i => new FieldSlot()).ToArray();
            old[4] = new FieldSlot { item = content.parts.idName, count = 2 };
            FieldCheckpoint.ApplyInventory(old, content);
            Check(FieldInventory.Slots.Count() == 28 && baseSlot.itemUI.slotInfo.itemInfo == null &&
                  FieldInventory.Count(content.parts) == 2, "open bag legacy slot migration");
            Check(FieldCheckpoint.CaptureInventory()[4].item == null, "regular migration cleared legacy save index");

            var dragged = FieldInventory.Slots.First(x => x.slotInfo.itemInfo == content.parts);
            var source = dragged.GetComponentsInChildren<Image>(true).First(x => x.name == "Icon").gameObject;
            baseSlot.SendMessage("OnDrop", new PointerEventData(EventSystem.current) { pointerDrag = source });
            Check(dragged.dropingSlot == dragged.parDragBefore, "dragged item returns to origin");
            dragged.dropingSlot = null;

            foreach (var entry in old) { entry.item = content.scrap.idName; entry.count = content.scrap.maxAmount; }
            old[4] = new FieldSlot { item = content.parts.idName, count = 2 };
            FieldCheckpoint.ApplyInventory(old, content);
            var full = FieldCheckpoint.CaptureInventory();
            Check(full.Length == 29 && full[4].count == 2 && full[4].item == content.parts.idName &&
                  baseSlot.itemUI.slotInfo.itemInfo == null && FieldInventory.Give(content.potion, 1) == 0,
                  "full inventory reserves slot five and preserves overflow");
            FieldCheckpoint.ApplyInventory(full, content);
            FieldInventory.Slots.Last().MakeEmptySlot();
            FieldInventory.RestoreLegacyStack(Managers.Inven.hotBarSlotInfo[4]);
            var merged = FieldCheckpoint.CaptureInventory();
            Check(merged[4].item == null && FieldInventory.Slots.Last().slotInfo.itemInfo == content.parts &&
                  FieldInventory.Count(content.parts) == 2, "overflow recovered once after save roundtrip");
            return "PASS: 4+24 item slots, drag rejection, free-space migration, full-bag legacy roundtrip and recovery";
        }
        finally
        {
            FieldCheckpoint.ApplyInventory(original, content);
            Managers.Inven.CheckHotBarChoice();
        }
    }
}
