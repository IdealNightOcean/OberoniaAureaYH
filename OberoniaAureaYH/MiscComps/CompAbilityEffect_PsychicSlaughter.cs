using RimWorld;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea;

public class CompAbilityEffect_PsychicSlaughter : RimWorld.CompAbilityEffect_PsychicSlaughter
{
    public override void PostApplied(List<LocalTargetInfo> targets, Map map)
    {
        base.PostApplied(targets, map);
        if (OberoniaAureaYHUtility.OAFaction == null)
        {
            return;
        }
        Faction oaFaction = OberoniaAureaYHUtility.OAFaction;
        foreach (LocalTargetInfo localTarget in targets)
        {
            if (localTarget.HasThing && localTarget.Thing is Pawn pawn)
            {
                Faction.OfPlayer.TryAffectGoodwillWith(oaFaction, -200, canSendMessage: false, canSendHostilityLetter: true, OA_HistoryEventDefOf.OA_PsychicSlaughter);
                if (Faction.OfPlayer.RelationKindWith(oaFaction) != FactionRelationKind.Hostile)
                {
                    Faction.OfPlayer.SetRelationDirect(oaFaction, FactionRelationKind.Ally, canSendHostilityLetter: true, reason: "OA_PsychicSlaughter");
                }
                if (pawn.RaceProps.Humanlike && pawn.Faction == oaFaction)
                {

                    GameComponent_OberoniaAurea oa_GCOA = OberoniaAureaYHUtility.OA_GCOA;
                    if (oa_GCOA != null)
                    {
                        oa_GCOA.assistPointsStoppageDays = 60;
                        oa_GCOA.UseAssistPoints(99999);
                    }
                    break;
                }
            }
        }
    }
}
