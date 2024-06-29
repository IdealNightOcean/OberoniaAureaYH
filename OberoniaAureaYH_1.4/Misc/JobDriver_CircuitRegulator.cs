using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace OberoniaAurea;

public class JobDriver_CircuitRegulator : JobDriver
{
    private static readonly int BaseJobEndInterval = 2500;
    private int jobEndInterval;
    private int ticksRemaining;
    private float repairSpeed = 1f;
    public float RepairSpeed
    {
        get
        {
            return repairSpeed;
        }
        set
        {
            repairSpeed = Mathf.Clamp(value, 0.01f, BaseJobEndInterval);
        }
    }
    private Thing CircuitRegulator => base.TargetThingA;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
        PathEndMode pathEndMode = PathEndMode.Touch;
        yield return Toils_Goto.GotoThing(TargetIndex.A, pathEndMode);
        Toil repair = ToilMaker.MakeToil("MakeNewToils");
        repair.initAction = delegate
        {
            RepairSpeed = repair.actor.GetStatValue(StatDefOf.ConstructionSpeed);
            jobEndInterval = Mathf.FloorToInt(BaseJobEndInterval / repairSpeed);
            ticksRemaining = jobEndInterval;
        };
        repair.tickAction = delegate
        {
            Pawn actor = repair.actor;
            ticksRemaining--;
            actor.skills.Learn(SkillDefOf.Construction, 0.1f);
            if (ticksRemaining <= 0f)
            {
                JobEffect();
                actor.records.Increment(RecordDefOf.ThingsRepaired);
                actor.jobs.EndCurrentJob(JobCondition.Succeeded);
            }
        };
        repair.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
        repair.WithEffect(EffecterDefOf.ConstructMetal, TargetIndex.A);
        repair.WithProgressBar(TargetIndex.A, () => Mathf.InverseLerp(jobEndInterval, 0f, ticksRemaining));
        repair.defaultCompleteMode = ToilCompleteMode.Never;
        repair.activeSkill = () => SkillDefOf.Construction;
        yield return repair;
    }
    private void JobEffect()
    {
        CompCircuitRegulator comp_CR = CircuitRegulator.TryGetComp<CompCircuitRegulator>();
        if (comp_CR != null)
        {
            comp_CR.CurCircuitStability = 1f;
        }
    }
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref jobEndInterval, "jobEndInterval", 2500);
        Scribe_Values.Look(ref ticksRemaining, "ticksRemaining", 0);
        Scribe_Values.Look(ref repairSpeed, "repairSpeed", 1f);
    }
}
