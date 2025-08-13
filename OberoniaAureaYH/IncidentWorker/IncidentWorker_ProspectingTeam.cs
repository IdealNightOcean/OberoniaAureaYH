using OberoniaAurea_Frame;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea;

public class IncidentWorker_ProspectingTeam : IncidentWorker_VisitorGroupBase
{
    protected override float TraderChance => 1f;
    protected override int? DurationTicks => 20000;

    private LordJob_VisitColonyTalkable tempLordJob;

    public override bool FactionCanBeGroupSource(Faction f, IncidentParms parms, bool desperate = false)
    {
        return f.IsOAFaction() && !f.HostileTo(Faction.OfPlayer);
    }

    protected override bool CanFireNowSub(IncidentParms parms)
    {
        if (ModUtility.OAFaction is null || ModUtility.OAFaction.HostileTo(Faction.OfPlayer))
        {
            return false;
        }
        return base.CanFireNowSub(parms);
    }

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        parms.faction = ModUtility.OAFaction;
        tempLordJob = null;
        bool result = base.TryExecuteWorker(parms);
        tempLordJob = null;
        return result;
    }

    protected override LordJob_VisitColonyBase CreateLordJob(IncidentParms parms, List<Pawn> pawns)
    {
        RCellFinder.TryFindRandomSpotJustOutsideColony(pawns[0], out IntVec3 result);
        tempLordJob = null;
        tempLordJob = new LordJob_VisitColonyTalkable(parms.faction, result, DurationTicks);
        return tempLordJob;
    }

    protected override List<Pawn> GeneratePawns(IncidentParms parms)
    {
        List<Pawn> pawns = [];
        Faction oaFaction = ModUtility.OAFaction;
        for (int i = 0; i < 4; i++)
        {
            PawnGenerationRequest genRequest = OAFrame_PawnGenerateUtility.CommonPawnGenerationRequest(OARK_PawnGenerateDefOf.OA_RK_Court_Member_Exploration, oaFaction, forceNew: true);
            Pawn pawn = PawnGenerator.GeneratePawn(genRequest);
            pawn.health.AddHediff(OARK_HediffDefOf.OARK_Hediff_ExplorationFatigue);
            pawns.Add(pawn);
        }
        return pawns;
    }

    protected override void PostTraderResolved(IncidentParms parms, List<Pawn> pawns, Pawn trader, bool traderExists)
    {
        trader ??= pawns.RandomElement();
        tempLordJob.SetTalkAction(trader, OARK_ModDefOf.OARK_Job_TalkWithProspectingLeader);
        tempLordJob = null;

        Find.LetterStack.ReceiveLetter(label: "OARK_LetterLabel_ProspectingTeam".Translate(),
                                       text: "OARK_Letter_ProspectingTeam".Translate(trader),
                                       textLetterDef: LetterDefOf.PositiveEvent,
                                       lookTargets: new LookTargets(pawns),
                                       relatedFaction: parms.faction);
    }
}
