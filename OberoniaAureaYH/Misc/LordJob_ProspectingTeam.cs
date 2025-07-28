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
    protected override LordToil_DefendPoint GetDefendPointLordToil()
    {
        return new LordToil_TalkWithColony(chillSpot);
    }
}

public class LordToil_TalkWithColony : LordToil_DefendPoint
{
    public LordToil_TalkWithColony(bool canSatisfyLongNeeds = true) : base(canSatisfyLongNeeds) { }
    public LordToil_TalkWithColony(IntVec3 defendPoint, float? defendRadius = null, float? wanderRadius = null) : base(defendPoint, defendRadius, wanderRadius) { }

    public override void Notify_PawnLost(Pawn victim, PawnLostCondition cond)
    {
        if (victim == OAInteractHandler.Instance.ProspectingLeader)
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
