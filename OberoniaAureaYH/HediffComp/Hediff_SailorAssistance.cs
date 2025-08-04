using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace OberoniaAurea;

[StaticConstructorOnStartup]
public class Hediff_SailorAssistance : HediffWithComps
{
    private static readonly TargetingParameters targetingParameters = new() { onlyTargetColonists = true };

    private int cooldownTicksLeft = -1;

    public override void TickInterval(int delta)
    {
        base.TickInterval(delta);
        if (cooldownTicksLeft > 0)
        {
            cooldownTicksLeft -= delta;
        }
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (Gizmo gizmo in base.GetGizmos())
        {
            yield return gizmo;
        }

        if (!pawn.Faction.IsPlayerSafe())
        {
            yield break;
        }

        Command_Target command_Invitation = new()
        {
            defaultLabel = "OARK_SailorAssistInvitation".Translate(),
            defaultDesc = "OARK_SailorAssistInvitationDesc".Translate(),
            icon = IconUtility.OADipIcon,
            targetingParams = targetingParameters,
            action = delegate (LocalTargetInfo inviter) { Invitation(pawn, inviter); }
        };
        if (cooldownTicksLeft > 0)
        {
            command_Invitation.Disable("WaitTime".Translate(cooldownTicksLeft.ToStringTicksToPeriod()));
        }
        else if (OAInteractHandler.Instance.AssistPoints < 50)
        {
            command_Invitation.Disable("OA_AlliancePointsNotEnough".Translate(50));
        }
        else if ((pawn.needs?.mood.CurLevelPercentage ?? 0f) < 0.9f)
        {
            command_Invitation.Disable();
        }

        yield return command_Invitation;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref cooldownTicksLeft, "cooldownTicksLeft", 0);
    }

    private static void Invitation(Pawn invitee, LocalTargetInfo inviter)
    {
        if (inviter.Thing is not Pawn inviterPawn)
        {
            return;
        }

        if (invitee.relations.OpinionOf(inviterPawn) < 20)
        {
            Messages.Message("OARk_NeedBetterBothwayOpinion".Translate(20), MessageTypeDefOf.RejectInput, historical: false);
            return;
        }
        else if (inviterPawn.relations.OpinionOf(invitee) < 20)
        {
            Messages.Message("OARk_NeedBetterBothwayOpinion".Translate(20), MessageTypeDefOf.RejectInput, historical: false);
            return;
        }
        else
        {
            Job job = JobMaker.MakeJob(OARK_ModDefOf.OARK_Job_SailorInvitation, invitee);
            job.count = 1;
            job.playerForced = true;
            inviterPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
        }
    }
}