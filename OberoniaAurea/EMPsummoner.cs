using RimWorld;
using Verse;
using Verse.AI;

namespace OberoniaAurea;

public class EMPsummoner : Apparel
{
	public override void Notify_BulletImpactNearby(BulletImpactData impactData)
	{
		Pawn wearer = base.Wearer;
		Thing launcher = impactData.bullet.Launcher;
		if (wearer != null && !wearer.Dead && launcher != null && launcher.HostileTo(base.Wearer) && !wearer.IsColonist && wearer.Spawned && (launcher is Building || (launcher is Pawn pawn && pawn.RaceProps.IsMechanoid)))
		{
			Verb verb = ((Thing)this).TryGetComp<CompApparelReloadable>().AllVerbs[0];
			if (verb != null && verb.Available() && verb.ValidateTarget(launcher))
			{
				Job job = JobMaker.MakeJob(JobDefOf.UseVerbOnThingStatic, launcher);
				job.verbToUse = ((Thing)this).TryGetComp<CompApparelReloadable>().AllVerbs[0];
				wearer.jobs.TryTakeOrderedJob(job, JobTag.Misc);
			}
		}
	}
}
