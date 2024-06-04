using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace OberoniaAurea;

//右键菜单：远行队到达多方会谈
public class CaravanArrivalAction_VisitMultiPartyTalks : CaravanArrivalAction
{
    private WorldObject_MultiPartyTalks multiPartyTalks;

    public override string Label => "OA_VisitMultiPartyTalks".Translate(multiPartyTalks.Label);

    public override string ReportString => "CaravanVisiting".Translate(multiPartyTalks.Label);

    public CaravanArrivalAction_VisitMultiPartyTalks()
    {
    }

    public CaravanArrivalAction_VisitMultiPartyTalks(WorldObject_MultiPartyTalks multiPartyTalks)
    {
        this.multiPartyTalks = multiPartyTalks;
    }

    public override FloatMenuAcceptanceReport StillValid(Caravan caravan, int destinationTile)
    {
        FloatMenuAcceptanceReport floatMenuAcceptanceReport = base.StillValid(caravan, destinationTile);
        if (!floatMenuAcceptanceReport)
        {
            return floatMenuAcceptanceReport;
        }
        if (multiPartyTalks != null && multiPartyTalks.Tile != destinationTile)
        {
            return false;
        }
        return CanVisit(caravan, multiPartyTalks);
    }

    public override void Arrived(Caravan caravan)
    {
        multiPartyTalks.Notify_CaravanArrived(caravan);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref multiPartyTalks, "multiPartyTalks");
    }

    public static FloatMenuAcceptanceReport CanVisit(Caravan caravan, WorldObject_MultiPartyTalks multiPartyTalks)
    {
        return multiPartyTalks?.Spawned ?? false;
    }

    public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan, WorldObject_MultiPartyTalks multiPartyTalks)
    {
        return CaravanArrivalActionUtility.GetFloatMenuOptions(() => CanVisit(caravan, multiPartyTalks), () => new CaravanArrivalAction_VisitMultiPartyTalks(multiPartyTalks), "OA_VisitMultiPartyTalks".Translate(multiPartyTalks.Label), caravan, multiPartyTalks.Tile, multiPartyTalks);
    }
}
