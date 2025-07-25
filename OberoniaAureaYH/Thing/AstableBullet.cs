using OberoniaAurea_Frame;
using RimWorld;
using Verse;

namespace OberoniaAurea;

public class AstableBullet : BulletBase
{
    protected override void ImpactPawn(Map map, Pawn hitPawn)
    {
        StaggerHandler staggerHandler = hitPawn?.stances?.stagger;
        if (staggerHandler is not null)
        {
            if (hitPawn.BodySize <= def.projectile.stoppingPower + 0.001f)
            {
                staggerHandler.StaggerFor(95);
            }
            staggerHandler.Notify_BulletImpact(this);
        }
    }
}
