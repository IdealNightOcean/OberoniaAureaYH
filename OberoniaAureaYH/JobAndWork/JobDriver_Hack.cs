using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace OberoniaAurea;

public class JobDriver_Hack : JobDriver
{
    private Thing HackTarget => TargetThingA;

    [Unsaved]
    private CompHackable compHackable;
    private CompHackable CompHackable => compHackable ??= HackTarget.TryGetComp<CompHackable>();
    private float hackingSpeed;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(HackTarget, job, 1, -1, null, errorOnFailed);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
        this.FailOn(() => CompHackable.Props.intellectualSkillPrerequisite > 0 && pawn.skills.GetSkill(SkillDefOf.Intellectual).Level < CompHackable.Props.intellectualSkillPrerequisite);
        PathEndMode pathEndMode = (TargetThingA.def.hasInteractionCell ? PathEndMode.InteractionCell : PathEndMode.ClosestTouch);
        yield return Toils_Goto.GotoThing(TargetIndex.A, pathEndMode);
        Toil toil = ToilMaker.MakeToil("MakeNewToils");
        toil.handlingFacing = true;
        toil.initAction = delegate
        {
            hackingSpeed = pawn.GetStatValue(StatDefOf.HackingSpeed);
        };
        toil.tickAction = delegate
        {
            CompHackable.DoHack(hackingSpeed);
            pawn.skills.Learn(SkillDefOf.Intellectual, 0.1f);
            pawn.rotationTracker.FaceTarget(HackTarget);
        };
        toil.WithEffect(EffecterDefOf.Hacking, TargetIndex.A);
        toil.WithProgressBar(TargetIndex.A, () => CompHackable.ProgressPercent, interpolateBetweenActorAndTarget: false, -0.5f, alwaysShow: true);
        toil.PlaySoundAtStart(SoundDefOf.Hacking_Started);
        toil.PlaySustainerOrSound(SoundDefOf.Hacking_InProgress);
        toil.AddFinishAction(delegate
        {
            if (CompHackable.IsHacked)
            {
                SoundDefOf.Hacking_Completed.PlayOneShot(HackTarget);
            }
            else
            {
                SoundDefOf.Hacking_Suspended.PlayOneShot(HackTarget);
            }
        });
        toil.FailOnCannotTouch(TargetIndex.A, pathEndMode);
        toil.FailOn(() => !CompHackable.IsHackable);
        toil.defaultCompleteMode = ToilCompleteMode.Never;
        toil.activeSkill = () => SkillDefOf.Intellectual;
        yield return toil;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref hackingSpeed, "hackingSpeed", defaultValue: 0f);
    }
}
