using RimWorld;
using Verse;
using Verse.AI;

namespace OberoniaAurea;

public class Verb_SummonEMP : Verb
{
    public override void OrderForceTarget(LocalTargetInfo target)
    {
        Job job = JobMaker.MakeJob(JobDefOf.UseVerbOnThingStatic, target);
        job.verbToUse = this;
        CasterPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
    }

    protected override bool TryCastShot()
    {
        if (currentTarget.HasThing && currentTarget.Thing.Map != caster.Map)
        {
            return false;
        }
        CompApparelReloadable reloadableCompSource = ReloadableCompSource;
        if (reloadableCompSource is null)
        {
            TryShot();
            return true;
        }
        else if (reloadableCompSource.CanBeUsed(out string _))
        {
            TryShot();
            reloadableCompSource.UsedOnce();
            return true;
        }
        return false;
    }
    protected void TryShot()
    {
        Thing thing = ThingMaker.MakeThing(verbProps.spawnDef);
        DropPodUtility.DropThingsNear(currentTarget.Cell, caster.Map, [thing], 60, faction: caster.Faction);
    }
}
