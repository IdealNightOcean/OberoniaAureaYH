using RimWorld;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea;

public class CompAbilityEffect_PsychicSlaughter : RimWorld.CompAbilityEffect_PsychicSlaughter
{
    public override void PostApplied(List<LocalTargetInfo> targets, Map map)
    {
        base.PostApplied(targets, map);
        if (ModUtility.OAFaction is null)
        {
            return;
        }
        Faction oaFaction = ModUtility.OAFaction;
        foreach (LocalTargetInfo localTarget in targets)
        {
            if (localTarget.HasThing && localTarget.Thing is Pawn pawn && pawn.RaceProps.Humanlike && pawn.Faction == oaFaction)
            {
                Faction.OfPlayer.TryAffectGoodwillWith(oaFaction, -200, canSendMessage: false, canSendHostilityLetter: true, OARK_HistoryEventDefOf.OA_PsychicSlaughter);
                if (Faction.OfPlayer.RelationKindWith(oaFaction) != FactionRelationKind.Hostile)
                {
                    Faction.OfPlayer.SetRelationDirect(oaFaction, FactionRelationKind.Hostile, canSendHostilityLetter: true, reason: "OA_PsychicSlaughter".Translate());
                }
                break;
            }
        }
    }
}
