using RimWorld;
using Verse;

namespace OberoniaAurea_Hyacinth;

public class CompAntiStealth : CompsApparelRangeEffect
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
            if (hyacinthGameComp.antiStealth.ContainsKey(pawn))
            {
                hyacinthGameComp.antiStealth[pawn] = Props.RadiusSquared;

            }
            else
            {
                hyacinthGameComp.antiStealth.Add(pawn, Props.RadiusSquared);
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
        hyacinthGameComp?.antiStealth.Remove(pawn);
    }
}

