using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea;

/*
public class CaravanArrivalAction_VisitSettlementDip : CaravanArrivalAction
{
    protected Settlement settlement;
    protected SettlementDipComp SettlementDipComp => settlement?.GetComponent<SettlementDipComp>();

    protected SettlementDipVisitType visitType = SettlementDipVisitType.None;

    public override string Label => "OA_VisitSettlement".Translate(settlement.Label);
    public override string ReportString => "CaravanVisiting".Translate(settlement.Label);
    public CaravanArrivalAction_VisitSettlementDip()
    { }

    public CaravanArrivalAction_VisitSettlementDip(Settlement settlement, SettlementDipVisitType visitType)
    {
        this.settlement = settlement;
        this.visitType = visitType;
    }

    public override FloatMenuAcceptanceReport StillValid(Caravan caravan, int destinationTile)
    {
        FloatMenuAcceptanceReport floatMenuAcceptanceReport = base.StillValid(caravan, destinationTile);
        if (!floatMenuAcceptanceReport)
        {
            return floatMenuAcceptanceReport;
        }
        if (settlement == null || !settlement.Spawned || settlement.Tile != destinationTile)
        {
            return false;
        }
        return CanVisit(settlement, visitType);
    }

    public override void Arrived(Caravan caravan)
    {
        SettlementDipComp?.Notify_CaravanArrived(caravan, visitType);
    }
    public static FloatMenuAcceptanceReport CanVisit(Settlement settlement, SettlementDipVisitType visitType)
    {
        SettlementDipComp OASettlementDipComp = settlement.GetComponent<SettlementDipComp>();
        if (OASettlementDipComp == null)
        {
            return false;
        }
        return OASettlementDipComp.CanVisitNow(visitType);
    }
    public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan, Settlement settlement, FloatMenuAcceptanceReport report, string label = null, SettlementDipVisitType visitType = SettlementDipVisitType.None)
    {
        return CaravanArrivalActionUtility.GetFloatMenuOptions(() => report, () => new CaravanArrivalAction_VisitSettlementDip(settlement, visitType), label ?? "OA_VisitSettlement".Translate(settlement.Label), caravan, settlement.Tile, settlement);
    }
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref settlement, "settlement");
        Scribe_Values.Look(ref visitType, "visitType", SettlementDipVisitType.None);
    }
}
*/