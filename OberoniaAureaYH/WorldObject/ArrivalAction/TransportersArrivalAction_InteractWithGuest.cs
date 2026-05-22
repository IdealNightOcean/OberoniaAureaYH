using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea;

public class TransportersArrivalAction_InteractWithGuest : TransportersArrivalAction_VisitSettlement
{
    public TransportersArrivalAction_InteractWithGuest() { }
    public TransportersArrivalAction_InteractWithGuest(Settlement settlement, string arrivalMessageKey) : base(settlement, arrivalMessageKey) { }

    public override FloatMenuAcceptanceReport StillValid(IEnumerable<IThingHolder> pods, PlanetTile destinationTile)
    {
        FloatMenuAcceptanceReport baseReport = base.StillValid(pods, destinationTile);
        if (!baseReport)
        {
            return baseReport;
        }
        return CanInteractWith(pods, settlement);
    }

    public override void Arrived(List<ActiveTransporterInfo> transporters, PlanetTile tile)
    {
        Pawn pawn = null;
        for (int i = 0; i < transporters.Count; i++)
        {
            foreach (Thing item in (IEnumerable<Thing>)transporters[i].GetDirectlyHeldThings())
            {
                if (item is Pawn p)
                {
                    pawn = p;
                    break;
                }
            }

            if (pawn is not null)
                break;
        }

        base.Arrived(transporters, tile);

        if (pawn is null)
            return;

        Caravan caravan = pawn.GetCaravan();
        if (caravan is null)
            return;

        WorldObjectComp_ExtraGuest guestComp = settlement.GetComponent<WorldObjectComp_ExtraGuest>();
        if (guestComp is null || guestComp.ExtraSiteGuest is null || !guestComp.ExtraSiteGuest.IsTraderActive)
            return;

        guestComp.InteractWithGuest(caravan);
    }

    public static FloatMenuAcceptanceReport CanInteractWith(IEnumerable<IThingHolder> pods, Settlement settlement)
    {
        if (!TransportersArrivalAction_VisitSettlement.CanVisit(pods, settlement))
            return false;

        if (settlement.HasMap)
            return false;

        if (settlement.Faction is null || settlement.Faction.def.permanentEnemy || settlement.Faction.HostileTo(Faction.OfPlayer))
            return false;

        WorldObjectComp_ExtraGuest guestComp = settlement.GetComponent<WorldObjectComp_ExtraGuest>();
        if (guestComp is null || guestComp.ExtraSiteGuest is null || !guestComp.ExtraSiteGuest.IsTraderActive)
            return false;

        foreach (IThingHolder pod in pods)
        {
            ThingOwner directlyHeldThings = pod.GetDirectlyHeldThings();
            if (pod is CompTransporter compTransporter && CaravanShuttleUtility.IsCaravanShuttle(compTransporter))
            {
                directlyHeldThings = compTransporter.parent.GetCaravan().GetDirectlyHeldThings();
            }
            foreach (Thing t in (IEnumerable<Thing>)directlyHeldThings)
            {
                if (t is not Pawn pawn)
                    continue;

                if (pawn.RaceProps.Humanlike && pawn.skills is not null && pawn.skills.GetSkill(SkillDefOf.Social).TotallyDisabled)
                    return true;
            }
        }

        return false;
    }

    public static new IEnumerable<FloatMenuOption> GetFloatMenuOptions(Action<PlanetTile, TransportersArrivalAction> launchAction, IEnumerable<IThingHolder> pods, Settlement settlement, bool isShuttle)
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
        return TransportersArrivalActionUtility.GetFloatMenuOptions(
            acceptanceReportGetter: () => CanVisit(pods, settlement),
            arrivalActionGetter: () => new TransportersArrivalAction_InteractWithGuest(settlement, isShuttle ? "MessageShuttleArrived" : "MessageTransportPodsArrived"),
            label: label,
            launchAction: launchAction,
            destinationTile: settlement.Tile);
    }
}
