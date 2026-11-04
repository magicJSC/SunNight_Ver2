using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public sealed class ExplorationState
{
    // Missing in v0.2 saves: keep zero so migration can identify them.
    public int layoutVersion;
    public bool firstNightCleared;
    public bool basementMapReceived;
    public bool sunNightJournalRead;
    public bool HasBasementMap => basementMapReceived || firstNightCleared;
    public List<string> research = new List<string>();
    public List<string> killedGuards = new List<string>();
    public List<ExplorationClaim> claims = new List<ExplorationClaim>();
    public bool Has(string id) => research.Contains(id);
    public int Claimed(string id) => claims.Find(x => x.id == id)?.amount ?? 0;
    public void AddClaim(string id, int amount)
    {
        if (amount <= 0) return;
        var claim = claims.Find(x => x.id == id);
        if (claim == null) claims.Add(new ExplorationClaim { id = id, amount = amount });
        else claim.amount += amount;
    }
}
[Serializable] public sealed class ExplorationClaim { public string id; public int amount; }

public sealed class FieldResearch
{
    public readonly string id, name, effect;
    public readonly int scrap, parts;
    public FieldResearch(string id, string name, int scrap, int parts, string effect)
    { this.id=id; this.name=name; this.scrap=scrap; this.parts=parts; this.effect=effect; }
    public static readonly FieldResearch[] All = {
        new FieldResearch("U1", "배트 보강 I", 6, 1, "배트 피해 6 → 9"),
        new FieldResearch("U2", "대포 포신 I", 8, 2, "모든 대포 피해 5 → 7"),
        new FieldResearch("U3", "방어판 I", 6, 1, "대포 HP·상한 +10 / 벽 +15 (기존 손상 유지)"),
        new FieldResearch("U4", "거점 프레임 I", 8, 2, "거점 HP·상한 +150 / 대포 상한 4 (손상 유지)")
    };
}
public static class FieldSession
{
    public static ExplorationState State = new ExplorationState();
    public static bool Active;
    public static bool ResumeRequested;
    public static int CannonLimit => State.Has("U4") ? 4 : 3;
    public static void NewRun() { State = new ExplorationState(); Active = true; ResumeRequested = false; FieldCheckpoint.Transfer = null; FieldCheckpoint.TransferPreparation = null; }
    public static bool CanBuy(FieldResearch research, int scrap, int parts) =>
        State.firstNightCleared && !State.Has(research.id) && scrap >= research.scrap && parts >= research.parts;
}
