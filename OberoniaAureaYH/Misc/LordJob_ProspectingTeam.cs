using OberoniaAurea_Frame;
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace OberoniaAurea;

public class LordJob_ProspectingTeam : LordJob_VisitColonyBase
{
    public LordJob_ProspectingTeam() : base() { }
    public LordJob_ProspectingTeam(Faction faction, IntVec3 chillSpot, int? durationTicks = null) : base(faction, chillSpot, durationTicks) { }

    public override void Notify_PawnLost(Pawn p, PawnLostCondition condition)
    {
        base.Notify_PawnLost(p, condition);
        if (p == OAInteractHandler.Instance.ProspectingLeader)
        {
            OAInteractHandler.Instance.ProspectingLeader = null;
        }
    }

    public override IEnumerable<FloatMenuOption> ExtraFloatMenuOptions(Pawn target, Pawn forPawn)
    {
        if (target == OAInteractHandler.Instance.ProspectingLeader)
        {
            yield return FloatMenuUtility.DecoratePrioritizedTask(option: new FloatMenuOption(label: "OARK_TalkWith".Translate(target.LabelShort),
                                                                                              action: delegate { Interact(target, forPawn); },
                                                                                              priority: MenuOptionPriority.InitiateSocial,
                                                                                              mouseoverGuiAction: null, revalidateClickTarget: target),
                                                                  pawn: forPawn,
                                                                  target: target);
        }
    }

    private static void Interact(Pawn target, Pawn forPawn)
    {
        Job job = JobMaker.MakeJob(OARK_ModDefOf.OARK_Job_TalkWithProspectingLeader, target);
        job.playerForced = true;
        forPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
    }
}