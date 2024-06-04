using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OberoniaAurea;
//研究峰会 - 参会旅行者到达
public class IncidentWorker_ResearchSummitTraveller : IncidentWorker_TraderCaravanArrival
{
    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        if (!TryFindResearchSummit(out WorldObject_ResearchSummit researchSummit))
        {
            return false;
        }
        if (base.TryExecuteWorker(parms))
        {
            Messages.Message("OA_Incident_RSTraveller".Translate(parms.faction.NameColored, researchSummit.AssociateWorldObject.Named("SETTLEMENT")), MessageTypeDefOf.NeutralEvent);
            return true;
        }
        return false;
    }
    private static bool TryFindResearchSummit(out WorldObject_ResearchSummit researchSummit)
    {
        var obj = Find.WorldObjects.AllWorldObjects.Where(w => w.def == OA_WorldObjectDefOf.OA_RK_ResearchSummit).RandomElementWithFallback(null);
        if (obj == null)
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
    protected override void SendLetter(IncidentParms parms, List<Pawn> pawns, TraderKindDef traderKind)
    { }
}