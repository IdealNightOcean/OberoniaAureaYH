using OberoniaAurea_Frame;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OberoniaAurea;

//研究峰会 - 参会旅行者商队
[StaticConstructorOnStartup]
public class IncidentWorker_ResearchSummitTraveller : IncidentWorker_VisitorGroupBase
{
    private static readonly SimpleCurve TravellerPointsCurve =
    [
        new CurvePoint(100f, 0f),
        new CurvePoint(200f, 1f),
        new CurvePoint(300f, 1f),
        new CurvePoint(400f, 0.5f),
        new CurvePoint(500f, 0.25f),
        new CurvePoint(1000f, 0.1f),
        new CurvePoint(2000f, 0.05f),
        new CurvePoint(2000f, 0f)
    ];

    private Settlement Settlement { get; set; }
    protected override float TraderChance => 1f;
    protected override TraderKindDef FixedTraderKind => OARK_PawnGenerateDefOf.OA_ResearchSummit_TravellerTrader;

    protected override void ResolveParmsPoints(IncidentParms parms)
    {
        parms.points = Rand.ByCurve(TravellerPointsCurve);
    }

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        Settlement = null;
        if (!TryFindResearchSummit(out WorldObject_ResearchSummit researchSummit))
        {
            return false;
        }
        Settlement = researchSummit.AssociateSettlement;
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

    protected override void PostTraderResolved(IncidentParms parms, List<Pawn> pawns, Pawn trader)
    {
        if (trader is not null)
        {
            Messages.Message("OA_ResearchSummitTraveller_Arrival".Translate(parms.faction.NameColored, Settlement.Named("SETTLEMENT")), trader, MessageTypeDefOf.NeutralEvent);
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
