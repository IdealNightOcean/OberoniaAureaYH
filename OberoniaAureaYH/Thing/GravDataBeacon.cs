using OberoniaAurea_Frame;
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.Sound;

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

        OAInteractHandler.Instance.CooldownManager.RegisterRecord("GravDataBeacon", cdTicks: 5 * 60000, shouldRemoveWhenExpired: false);

        if (!OAInteractHandler.Instance.CooldownManager.IsInCooldown("SDFriendlyEvent"))
        {
            OAInteractHandler.Instance.CooldownManager.RegisterRecord("SDFriendlyEvent", cdTicks: 10 * 60000, shouldRemoveWhenExpired: true);
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

        Find.LetterStack.ReceiveLetter(label: "OARK_LetterLabel_GravDataBeaconLanded".Translate(),
                                       text: "OARK_Letter_GravDataBeaconLanded".Translate(),
                                       textLetterDef: LetterDefOf.NeutralEvent,
                                       lookTargets: this);
    }


    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (Gizmo gizmo in base.GetGizmos())
        {
            yield return gizmo;
        }

        if (Spawned)
        {
            Command_Toggle command_Active = new()
            {
                defaultLabel = isActive ? "Close".Translate() : "Activate".Translate(),
                defaultDesc = isActive ? "OARK_DeativeGravDataBeaconDesc".Translate(Label) : "OARK_ActiveGravDataBeaconDesc".Translate(Label),
                icon = TexCommand.DesirePower,
                isActive = () => isActive,
                toggleAction = delegate
                {
                    isActive = !isActive;
                    OARK_RimWorldDefOf.Emp_Crack.PlayOneShot(SoundInfo.InMap(this));
                }
            };
            if (!isActive)
            {
                AcceptanceReport report = CanActivateNow(resultOnly: false);
                if (!report)
                {
                    command_Active.Disable(report.Reason);
                }
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

        int cooldownTicksLeft = OAInteractHandler.Instance.CooldownManager.GetCooldownTicksLeft("GravDataBeacon");
        if (cooldownTicksLeft > 0)
        {
            return resultOnly ? false : "WaitTime".Translate(cooldownTicksLeft.ToStringTicksToPeriod());
        }

        return true;
    }

}
