using RimWorld;
using Verse;

namespace OberoniaAurea_Hyacinth;

public class CompRatkinHarmTurret : CompsApparelRangeEffect
{
    protected override void RegisterGameComp(Pawn pawn)
    {
        if (pawn == null || (!Props.aiEffectiveUse && pawn.Faction != Faction.OfPlayer))
        {
            return;
        }
        GameComponent_Hyacinth hyacinthGameComp = Hyacinth_Utility.HyacinthGameComp;
        if (hyacinthGameComp != null)
        {
            if (hyacinthGameComp.ratkinHarmTurret.ContainsKey(pawn))
            {
                hyacinthGameComp.ratkinHarmTurret[pawn] = Props.RadiusSquared;

            }
            else
            {
                hyacinthGameComp.ratkinHarmTurret.Add(pawn, Props.RadiusSquared);
            }
        }
    }
    protected override void UnregisterGameComp(Pawn pawn)
    {
        if (pawn == null)
        {
            return;
        }
        GameComponent_Hyacinth hyacinthGameComp = Hyacinth_Utility.HyacinthGameComp;
        hyacinthGameComp?.ratkinHarmTurret.Remove(pawn);
    }
}