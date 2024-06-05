using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea;
public class ResearchSummit_Fair : WorldObject_InteractiveBase
{
    public SimpleTrader innerTrader;

    public static SimpleTrader GenerateFairTrader(int tile)
    {
        SimpleTrader trader = new(OA_PawnGenerateDefOf.OA_ResearchSummit_FairTrader)
        {
            WasAnnounced = false
        };
        trader.GenerateThings(tile);
        return trader;
    }
    protected static void GenerateThings(ThingOwner things, TraderKindDef def, int tile)
    {
        ThingSetMakerParams parms = default;
        parms.traderDef = def;
        parms.tile = tile;
        things.TryAddRangeOrTransfer(ThingSetMakerDefOf.TraderStock.root.Generate(parms));
    }
    public override void Notify_CaravanArrived(Caravan caravan)
    {
        Pawn pawn = BestCaravanPawnUtility.FindBestNegotiator(caravan);
        if (pawn == null)
        {
            Messages.Message("OA_MessageNoTrader".Translate(), caravan, MessageTypeDefOf.NegativeEvent, historical: false);
            return;
        }
        Find.WindowStack.Add(new Dialog_Trade(pawn, innerTrader));
    }

    public override void PostRemove()
    {
        innerTrader = null;
        base.PostRemove();
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