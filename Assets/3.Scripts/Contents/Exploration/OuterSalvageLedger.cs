using System;
using System.Linq;
using UnityEngine;

// Small, guaranteed finds become one existing crafting resource. Claims are checkpointed.
public static class OuterSalvageLedger
{
    public const int PiecesPerScrap=10;
    public static readonly string[] Sources={"D0_s2","D0_s3","D0_s4","D0_s5","E0","E0_s1","E0_s2","E0_s3","D1","D2","E1","E2"};
    public static bool Enabled(ExplorationContent c)=>c.outerSalvage!=null && c.outerSalvage.Length>0;
    public static bool Replaced(ExplorationContent c,FieldRewardDefinition d,int index)=>
        Enabled(c) && !d.salvageFragments && d.items[index]=="IronOre" && Sources.Contains(d.id);
    public static int Total(ExplorationState state,ExplorationContent c)=>
        c.outerSalvage.Sum(d=>Mathf.Min(d.amounts[0],state.Claimed(d.id+":0")));
    public static void Migrate(ExplorationState state,ExplorationContent c)
    {
        if(!Enabled(c) || state.layoutVersion>=4) return;
        int paid=c.rewards.Where(d=>Sources.Contains(d.id)).Sum(d=>
            Enumerable.Range(0,d.items.Length).Where(i=>d.items[i]=="IronOre")
                .Sum(i=>Mathf.Min(d.amounts[i],state.Claimed(d.id+":"+i))));
        int pieces=paid*PiecesPerScrap;
        foreach(var d in c.outerSalvage)
        {
            int take=Mathf.Min(pieces,d.amounts[0]);
            state.AddClaim(d.id+":0",take);pieces-=take;
        }
        state.layoutVersion=4;
    }
    // Transaction: if a newly completed scrap cannot fit, leave both pieces and payout untouched.
    public static bool TryCollect(ExplorationState state,ExplorationContent c,FieldRewardDefinition d,
        Func<int,int> give,out int pieces,out int scrap)
    {
        pieces=Mathf.Max(0,d.amounts[0]-state.Claimed(d.id+":0"));scrap=0;
        if(pieces==0) return false;
        int before=Total(state,c);
        scrap=(before+pieces)/PiecesPerScrap-before/PiecesPerScrap;
        // Each authored node has at most PiecesPerScrap pieces, so payout is zero or one item.
        if(scrap>1) throw new InvalidOperationException("Outer salvage node exceeds atomic payout budget");
        if(scrap>0 && give(scrap)!=scrap) return false;
        state.AddClaim(d.id+":0",pieces);
        return true;
    }
}
