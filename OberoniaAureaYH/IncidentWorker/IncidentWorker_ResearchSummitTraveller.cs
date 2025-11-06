using OberoniaAurea_Frame;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OberoniaAurea;

//研究峰会 - 参会旅行者到达
[StaticConstructorOnStartup]
public class IncidentWorker_ResearchSummitTraveller : IncidentWorker_VisitorGroupBase
{
    protected override TraderKindDef FixedTraderKind => OARK_PawnGenerateDefOf.OA_ResearchSummit_TravellerTrader;

    private static readonly SimpleCurve TravellerPointsCurve =
    [
        new CurvePoint(0f, 200f),
        new CurvePoint(10000f, 500f)
    ];

    private Settlement settlement;

    protected override void ResolveParmsPoints(IncidentParms parms)
    {
        if (parms.points < 0f)
        {
            parms.points = Rand.ByCurve(TravellerPointsCurve);
        }
    }

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        settlement = null;
        if (!TryFindResearchSummit(out WorldObject_ResearchSummit researchSummit))
        {
            return false;
        }
        settlement = researchSummit.AssociateSettlement;
        return base.TryExecuteWorker(parms);
    }

    protected override List<Pawn> GeneratePawns(IncidentParms parms)
    {
        PawnGroupMakerParms groupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDef, parms, ensureCanGenerateAtLeastOnePawn: true);
        if (!OARK_PawnGenerateDefOf.OA_ResearchSummit_TravellerMaker.TryGetRandomAvailablePawnGroupMaker(groupMakerParms, out PawnGroupMaker groupMaker))
        {
            return null;
        }

        return OAFrame_PawnGenerateUtility.GeneratePawns(groupMakerParms, groupMaker, warnOnZeroResults: false).ToList();
    }

    protected override void PostTraderResolved(IncidentParms parms, List<Pawn> pawns, Pawn trader, bool traderExists)
    {
        if (traderExists)
        {
            Messages.Message("OA_ResearchSummitTraveller_Arrival".Translate(parms.faction.NameColored, settlement.Named("SETTLEMENT")), trader, MessageTypeDefOf.NeutralEvent);
        }
    }
    private static bool TryFindResearchSummit(out WorldObject_ResearchSummit researchSummit)
    {
        WorldObject obj = Find.WorldObjects.AllWorldObjects.Where(w => w.def == OARK_WorldObjectDefOf.OA_RK_ResearchSummit).RandomElementWithFallback(null);
        if (obj is null)
        {
            researchSummit = null;
            return false;
        }
        else
        {
            researchSummit = (WorldObject_ResearchSummit)obj;
            return true;
        }
    }

}
