"""Validate recorded Play Mode evidence and package the selected runs for review."""
import json
import pathlib
import re
import shutil

SOURCE = pathlib.Path("Logs/PostNightV03")
DEST = pathlib.Path("docs/level-design")


def read(name):
    return json.loads((SOURCE / name).read_text(encoding="utf-8-sig"))


def lines(name):
    return [json.loads(s) for s in (SOURCE / name).read_text(encoding="utf-8-sig").splitlines() if s]


def inventory(entries):
    result = {}
    for entry in entries:
        if entry["item"] and entry["count"]:
            result[entry["item"]] = result.get(entry["item"], 0) + entry["count"]
    return result


def facilities(entries):
    return sorted((e["item"], e["cell"], e["hp"], e["max"]) for e in entries)


def summarize():
    earned = read("earned-postnight.json")
    entry = read("final-pre-factory.json")
    clear = read("final-clear.json")
    samples = lines("post-bat-fix-samples.jsonl")
    events = lines("post-bat-fix-events.jsonl")
    transfer = next(e for e in events if e["phase"] == "factory transferred")
    battle = [e for e in samples if e.get("bossPhase")]
    ready = battle[0]
    checks = {
        "paid_resource_ledger": inventory(entry["slots"]) == {"Bat": 1, "IronOre": 14, "MaintenancePart": 2, "HpPotion": 3},
        "three_optional_researches": set(entry["state"]["research"]) == {"U1", "U2", "U3"},
        "transition_inventory_unchanged": inventory(entry["slots"]) == inventory(transfer["inventory"]),
        "transition_player_hp_unchanged": entry["hp"] == transfer["hp"],
        "transition_tower_hp_unchanged": entry["towerHp"] == transfer["towerHp"],
        "transition_facilities_unchanged": sorted((e["item"], f'({e["x"]}, {e["y"]}, 0)', e["hp"]) for e in entry["buildings"]) == sorted((e["item"], e["cell"], e["hp"]) for e in transfer["facilities"]),
        "no_boss_body_facility_damage": all(facilities(e["facilities"]) == facilities(ready["facilities"]) for e in battle) and facilities(clear["facilities"]) == facilities(ready["facilities"]),
        "hunger_preserved_through_clear": earned["hunger"] == entry["hunger"] == clear["hunger"],
        "clear_inventory_unchanged": inventory(clear["inventory"]) == inventory(entry["slots"]),
        "all_nine_facilities_survive": len(clear["facilities"]) == 9 and all(e["hp"] > 0 for e in clear["facilities"]),
        "final_console_no_errors": read("final-console.json")["data"]["result"]["groundTruth"]["consoleErrors"] == 0,
    }
    assert all(checks.values()), checks
    u3 = read("research-restore-checks.json")
    u4 = read("u4-restore-checks.json")
    assert all(u3["reloadPreserves"].values()) and u3["paidU3CapacityAddedOnce"]
    assert all(u4.values())
    log = (SOURCE / "Editor-replay.log").read_text(encoding="utf-8", errors="replace")
    kills = re.findall(r"FOXTON_AUDIT stopped bossHp=0 seconds=([\d.]+)", log)
    assert len(kills) >= 2
    result = {
        "method": "Automated real-time Unity Play Mode inputs, real animation and collision. Known routes and telegraph reaction; not human blind play.",
        "checkpoint_limitation": "Prior exact post-night save unavailable. Replayed earned pre-night save. Three walls survived, versus two in the earlier session report.",
        "checks": checks,
        "research_U3": u3,
        "research_U4_separate_trial": u4,
        "boss_seconds_before_bat_fix": float(kills[0]),
        "boss_seconds_after_bat_fix": float(kills[1]),
        "factory_entry_to_clear_seconds": clear["seconds"],
        "final": clear,
        "console": read("final-console.json")["data"]["result"]["groundTruth"],
    }
    if (SOURCE / "mistake-clear.json").exists():
        mistake = read("mistake-clear.json")
        hits = [e for e in lines("mistake-events.jsonl") if e["phase"].startswith("controlled actual hit")]
        result["controlled_mistakes"] = {"hits": [{"event": e["phase"], "hp": e["hp"]} for e in hits], "final": mistake, "boss_seconds": float(kills[-1])}
        assert [e["phase"] for e in hits] == [f"controlled actual hit pattern={p} damage={d}" for p, d in [(1, 20), (2, 30), (3, 20)]]
        assert mistake["boss"] is None and mistake["hp"] > 0
        result["controlled_mistakes"]["console_before_reload_fix"] = read("mistake-console.json")["data"]["result"]["groundTruth"]
    if (SOURCE / "reload-console.json").exists():
        result["reload_after_fix_console"] = read("reload-console.json")["data"]["result"]["groundTruth"]
        assert result["reload_after_fix_console"]["consoleErrors"] == 0
        rebuilt = read("reload-building-check.json")
        assert inventory(rebuilt["slots"]) == {"Bat": 1, "IronOre": 1}
        assert len(rebuilt["buildings"]) == 9
        result["reload_then_paid_wall_construction"] = True
    DEST.mkdir(exist_ok=True)
    if (SOURCE / "final-compile.json").exists():
        compilation = read("final-compile.json")["data"]["result"]
        assert compilation["status"] == "completed" and not compilation["failed"]
        result["final_compile_after_removing_temporary_logs"] = compilation
    result["remaining_shutdown_issue"] = "After stopping Play Mode, one cleanup error named @Managers and @SoundRoot. This is separate from the zero-error combat/reload runs; global manager shutdown lifetime was not changed."
    names = ["earned-postnight.json", "night-replay.json", "final-pre-factory.json", "final-clear.json", "boss-before.json", "research-restore-checks.json", "u4-restore-checks.json", "final-events.jsonl", "post-bat-fix-events.jsonl", "post-bat-fix-samples.jsonl", "mistake-clear.json", "mistake-events.jsonl", "reload-building-check.json", "pattern-1.png", "pattern-2.png", "pattern-3.png"]
    for name in names:
        if (SOURCE / name).exists():
            shutil.copyfile(SOURCE / name, DEST / ("v03-postnight-" + name))
    (DEST / "v03-postnight-verification.json").write_text(json.dumps(result, ensure_ascii=False, indent=2), encoding="utf-8")
    print(json.dumps({"checks": checks, "bossSeconds": result["boss_seconds_after_bat_fix"], "hp": clear["hp"], "console": result["console"]}, indent=2))


if __name__ == "__main__":
    summarize()
