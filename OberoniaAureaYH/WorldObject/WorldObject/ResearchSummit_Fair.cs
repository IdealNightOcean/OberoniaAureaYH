using OberoniaAurea_Frame;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea;
public class ResearchSummit_Fair : WorldObject_InteractiveBase
{
    protected SiteTrader innerTrader;

    public void InitInnerTrader()
    {
        innerTrader = new(OA_PawnGenerateDefOf.OA_ResearchSummit_FairTrader, this);
        innerTrader.GenerateThings(this.Tile);
    }
    public override void Tick()
    {
        base.Tick();
        innerTrader?.Traderick();
    }
    public override void Notify_CaravanArrived(Caravan caravan)
    {
        Pawn pawn = BestCaravanPawnUtility.FindBestNegotiator(caravan);
        if (pawn == null)
        {
            Messages.Message("OAFrame_MessageNoTrader".Translate(), caravan, MessageTypeDefOf.NegativeEvent, historical: false);
            return;
        }
        Find.WindowStack.Add(new Dialog_Trade(pawn, innerTrader));
    }

    public override void Destroy()
    {
        innerTrader?.Destory();
        base.Destroy();
        
    }

    public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
    {
        foreach (FloatMenuOption floatMenuOption in base.GetFloatMenuOptions(caravan))
        {
            yield return floatMenuOption;
        }
        foreach (FloatMenuOption floatMenuOption2 in CaravanArrivalAction_VisitInteractiveObject.GetFloatMenuOptions(caravan, this, "OA_Visit_RSFair".Translate(this.Label)))
        {
            yield return floatMenuOption2;
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Deep.Look(ref innerTrader, "innerTrader");
    }

}