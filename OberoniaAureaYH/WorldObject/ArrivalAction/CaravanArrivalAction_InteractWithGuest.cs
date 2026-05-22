using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea;

public class CaravanArrivalAction_InteractWithGuest : CaravanArrivalAction
{
    private Settlement settlement;

    public override string Label => "OARK_VisitExtraGuest".Translate();
    public override string ReportString => "OARK_VisitingExtraGuest".Translate();

    public CaravanArrivalAction_InteractWithGuest() { }
    public CaravanArrivalAction_InteractWithGuest(Settlement settlement) => this.settlement = settlement;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref settlement, nameof(settlement));
    }

    public override FloatMenuAcceptanceReport StillValid(Caravan caravan, PlanetTile destinationTile)
    {
        FloatMenuAcceptanceReport baseReport = base.StillValid(caravan, destinationTile);
        if (!baseReport)
            return baseReport;

        if (settlement is null)
            return false;

        if (settlement is not null && settlement.Tile != destinationTile)
            return false;

        return CanInteractWith(caravan, settlement);
    }

    public override void Arrived(Caravan caravan)
    {
        WorldObjectComp_ExtraGuest guestComp = settlement.GetComponent<WorldObjectComp_ExtraGuest>();
        if (guestComp is null || guestComp.ExtraSiteGuest is null || !guestComp.ExtraSiteGuest.IsTraderActive)
            return;

        guestComp.InteractWithGuest(caravan);
    }

    public static FloatMenuAcceptanceReport CanInteractWith(Caravan caravan, Settlement settlement)
    {
        if (settlement is null)
            return false;

        if (!settlement.Spawned || settlement.HasMap)
            return false;

        if (settlement.Faction is null || settlement.Faction.def.permanentEnemy || settlement.Faction.IsPlayerSafe() || settlement.Faction.HostileTo(Faction.OfPlayer))
            return false;

        WorldObjectComp_ExtraGuest guestComp = settlement.GetComponent<WorldObjectComp_ExtraGuest>();
        if (guestComp is null || guestComp.ExtraSiteGuest is null || !guestComp.ExtraSiteGuest.IsTraderActive)
            return false;

        foreach (Pawn p in caravan.PawnsListForReading)
        {
            if (p.RaceProps.Humanlike && p.skills is not null && !p.skills.GetSkill(SkillDefOf.Social).TotallyDisabled)
                return true;
        }

        return false;

    }

    public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan, Settlement settlement)
    {
        WorldObjectComp_ExtraGuest guestComp = settlement.GetComponent<WorldObjectComp_ExtraGuest>();
        string label;
        if (guestComp is null || guestComp.ExtraSiteGuest is null || !guestComp.ExtraSiteGuest.IsTraderActive)
        {
            label = "OARK_VisitExtraGuest".Translate();
        }
        else
        {
            label = "OARK_VisitExtraGuestInfo".Translate(guestComp.ExtraSiteGuest.GuestFaction.Named(KeyLibrary_NamedArgument.FACTION));
        }
        return CaravanArrivalActionUtility.GetFloatMenuOptions(acceptanceReportGetter: () => CanInteractWith(caravan, settlement),
                                                               arrivalActionGetter: () => new CaravanArrivalAction_InteractWithGuest(settlement),
                                                               label: label,
                                                               caravan: caravan,
                                                               pathDestination: settlement.Tile,
                                                               revalidateWorldClickTarget: settlement);
    }
}