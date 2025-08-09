﻿using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace OberoniaAurea;

public abstract class JobDriver_InteractWithThingAtOnce : JobDriver
{
    protected Thing InteractTarget => TargetThingA;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedOrNull(TargetIndex.A);
        yield return Toils_Goto.GotoCell(TargetIndex.A, InteractTarget.def.hasInteractionCell ? PathEndMode.InteractionCell : PathEndMode.ClosestTouch);
        Toil interact = ToilMaker.MakeToil("MakeNewToils");
        interact.initAction = delegate
        {
            InteractAction(pawn);
        };
        yield return interact;
    }

    protected abstract void InteractAction(Pawn pawn);
}