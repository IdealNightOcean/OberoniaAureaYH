using RimWorld;
using Verse;
using Verse.AI;

namespace OberoniaAurea;

public class EMP_Summoner : Apparel
{
    public override void Notify_BulletImpactNearby(BulletImpactData impactData)
    {
        Pawn wearer = Wearer;
        if (wearer is null || wearer.Dead || !wearer.Spawned || wearer.IsColonist)
        {
            return;
        }
        Thing launcher = impactData.bullet.Launcher;
        if (launcher is not null && launcher.HostileTo(wearer) && (launcher is Building || (launcher is Pawn pawn && pawn.RaceProps.IsMechanoid)))
        {
            Verb verb = this.TryGetComp<CompApparelReloadable>().AllVerbs[0];
            if (verb is not null && verb.Available() && verb.ValidateTarget(launcher))
            {
                Job job = JobMaker.MakeJob(JobDefOf.UseVerbOnThingStatic, launcher);
                job.verbToUse = this.TryGetComp<CompApparelReloadable>().AllVerbs[0];
                wearer.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            }
        }
    }
}
