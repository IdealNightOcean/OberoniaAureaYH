using RimWorld;
using UnityEngine;
using Verse;

namespace OberoniaAurea;
public class BarrageEnhancedBullet : Bullet
{
    public override void Launch(Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventFriendlyFire = false, Thing equipment = null, ThingDef targetCoverDef = null)
    {
        base.Launch(launcher, origin, usedTarget, intendedTarget, hitFlags, preventFriendlyFire, equipment, targetCoverDef);
        float DamageMultiplier = equipment.TryGetComp<CompBarrageEnhancedFirepower>()?.DamageMultiplier ?? 0f;
        this.weaponDamageMultiplier = weaponDamageMultiplier + DamageMultiplier;
    }
}
