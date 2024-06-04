using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace OberoniaAurea;

//右键菜单：远行队到达外交斡旋
public class CaravanArrivalAction_VisitDiplomaticOffices : CaravanArrivalAction
{
    private WorldObject_DiplomaticOffices diplomaticOffices;

    public override string Label => "OA_VisitDiplomaticOffices".Translate(diplomaticOffices.Label);

    public override string ReportString => "CaravanVisiting".Translate(diplomaticOffices.Label);

    public CaravanArrivalAction_VisitDiplomaticOffices()
    {
    }

    public CaravanArrivalAction_VisitDiplomaticOffices(WorldObject_DiplomaticOffices diplomaticOffices)
    {
        this.diplomaticOffices = diplomaticOffices;
    }

    public override FloatMenuAcceptanceReport StillValid(Caravan caravan, int destinationTile)
    {
        FloatMenuAcceptanceReport floatMenuAcceptanceReport = base.StillValid(caravan, destinationTile);
        if (!floatMenuAcceptanceReport)
        {
            return floatMenuAcceptanceReport;
        }
        if (diplomaticOffices != null && diplomaticOffices.Tile != destinationTile)
        {
            return false;
        }
        return CanVisit(caravan, diplomaticOffices);
    }

    public override void Arrived(Caravan caravan)
    {
        diplomaticOffices.Notify_CaravanArrived(caravan);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref diplomaticOffices, "diplomaticOffices");
    }

    public static FloatMenuAcceptanceReport CanVisit(Caravan caravan, WorldObject_DiplomaticOffices diplomaticOffices)
    {
        return diplomaticOffices?.Spawned ?? false;
    }

    public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan, WorldObject_DiplomaticOffices diplomaticOffices)
    {
        return CaravanArrivalActionUtility.GetFloatMenuOptions(() => CanVisit(caravan, diplomaticOffices), () => new CaravanArrivalAction_VisitDiplomaticOffices(diplomaticOffices), "OA_VisitDiplomaticOffices".Translate(diplomaticOffices.Label), caravan, diplomaticOffices.Tile, diplomaticOffices);
    }
}
