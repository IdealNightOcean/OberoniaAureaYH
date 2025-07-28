using OberoniaAurea_Frame;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea;

public class GravDataBeacon : Building
{
    public const string TakenEffectSignal = "GravDataBeaconTakenEffect";

    private bool isActive;


    public override void PostSwapMap()
    {
        base.PostSwapMap();
        if (isActive && Spawned)
        {
            isActive = false;
            DataBeaconTakenEffect();
        }
    }

    private void DataBeaconTakenEffect()
    {
        ScienceDepartmentInteractHandler.Instance.AddGravTechPoints(1000, byPlayer: true);
        ScienceDepartmentInteractHandler.Instance.AdjustGravTechAssistPoint(150);

        QuestUtility.SendQuestTargetSignals(questTags, TakenEffectSignal, this.Named("SUBJECT"));
        Find.SignalManager.SendSignal(new Signal(TakenEffectSignal, new SignalArgs(this.Named("SUBJECT")), global: true));

        OAInteractHandler.Instance.RegisterCDRecord("GravDataBeacon", cdTicks: 5 * 60000);

        if (!OAInteractHandler.Instance.IsInCooldown("SDFriendlyEvent"))
        {
            OAInteractHandler.Instance.RegisterCDRecord("SDFriendlyEvent", cdTicks: 10 * 60000);
            IncidentParms parms = new()
            {
                target = Find.World
            };
            OAFrame_MiscUtility.AddNewQueuedIncident(OARK_IncidentDefOf.OARK_ScienceDepartmentFriendly, Rand.RangeInclusive(8, 14) * 60000, parms);
        }

        Map map = MapHeld;
        IncidentParms raidParms = new()
        {
            target = map,
            faction = Faction.OfMechanoids,
            raidStrategy = RaidStrategyDefOf.ImmediateAttack,
            points = StorytellerUtility.DefaultThreatPointsNow(map),
            forced = true
        };
        OAFrame_MiscUtility.AddNewQueuedIncident(IncidentDefOf.RaidEnemy, 5000, raidParms);
    }


    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (Gizmo gizmo in base.GetGizmos())
        {
            yield return gizmo;
        }

        if (!isActive && Spawned)
        {
            Command_Action command_Active = new()
            {
                defaultLabel = "Active".Translate(),
                defaultDesc = "OARK_ActiveGravDataBeaconDesc".Translate(),
                icon = null,
                action = delegate { isActive = true; }
            };
            AcceptanceReport report = CanActivateNow(resultOnly: false);
            if (!report)
            {
                command_Active.Disable(report.Reason);
            }
            yield return command_Active;
        }
    }

    public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
    {
        if (!BeingTransportedOnGravship)
        {
            isActive = false;
        }
        base.DeSpawn(mode);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref isActive, "isActive", defaultValue: false);
    }

    private static AcceptanceReport CanActivateNow(bool resultOnly)
    {
        if (!ModsConfig.OdysseyActive || ScienceDepartmentInteractHandler.Instance is null)
        {
            return false;
        }

        int cooldownTicksLeft = OAInteractHandler.Instance.GetCooldownTicksLeft("GravDataBeacon");
        if (cooldownTicksLeft > 0)
        {
            return resultOnly ? false : "OARK_GravDataBeacon_Cooling".Translate(cooldownTicksLeft.ToStringTicksToPeriod());
        }

        return true;
    }

}
