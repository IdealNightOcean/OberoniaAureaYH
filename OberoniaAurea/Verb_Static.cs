using RimWorld;
using Verse;
using Verse.AI;

namespace OberoniaAurea;

public class Verb_Static : Verb
{
	public override void OrderForceTarget(LocalTargetInfo target)
	{
		Job job = JobMaker.MakeJob(JobDefOf.UseVerbOnThingStatic, target);
		job.verbToUse = this;
		CasterPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
	}

	protected override bool TryCastShot()
	{
		if (base.ReloadableCompSource != null && base.ReloadableCompSource.CanBeUsed(out string _) && CastShot())
		{
			base.ReloadableCompSource.UsedOnce();
			return true;
		}
		return false;
	}

	protected virtual bool CastShot()
	{
		return true;
	}
}
