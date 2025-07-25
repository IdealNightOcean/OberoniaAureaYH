using RimWorld;
using UnityEngine;
using Verse;

namespace OberoniaAurea;
public class BarrageEnhancedBullet : Bullet
{

    private float damageMultiplier = 1f;
    public override int DamageAmount => Mathf.RoundToInt(base.DamageAmount * damageMultiplier);

    public override void Launch(Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventFriendlyFire = false, Thing equipment = null, ThingDef targetCoverDef = null)
    {
        damageMultiplier = 1f + (equipment?.TryGetComp<CompBarrageEnhancedFirepower>()?.DamageMultiplier ?? 0f);
        base.Launch(launcher, origin, usedTarget, intendedTarget, hitFlags, preventFriendlyFire, equipment, targetCoverDef);
    }
}
