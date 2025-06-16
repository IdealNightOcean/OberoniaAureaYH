using Verse;

namespace OberoniaAurea;

public class AstableExplosiveBullet : Projectile_Explosive
{
    protected override void Explode()
    {
        Map map = base.Map;
        IntVec3 position = base.Position;
        if (def.projectile.explosionEffect is not null)
        {
            Effecter effecter = def.projectile.explosionEffect.Spawn();
            effecter.Trigger(new TargetInfo(position, map), new TargetInfo(position, map));
            effecter.Cleanup();
        }
        int damAmount = (int)Rand.Range(DamageAmount * 0.4f, DamageAmount * 1.2f);
        float direction = origin.AngleToFlat(destination);

        GenExplosion.DoExplosion(center: position,
            map: map,
            radius: def.projectile.explosionRadius,
            damType: def.projectile.damageDef,
            instigator: launcher,
            damAmount: damAmount,
            armorPenetration: ArmorPenetration,
            explosionSound: def.projectile.soundExplode,
            weapon: equipmentDef,
            projectile: def,
            intendedTarget: intendedTarget.Thing,
            postExplosionSpawnThingDef: def.projectile.postExplosionSpawnThingDef,
            postExplosionSpawnChance: def.projectile.postExplosionSpawnChance,
            postExplosionSpawnThingCount: def.projectile.postExplosionSpawnThingCount,
            postExplosionGasType: def.projectile.postExplosionGasType,
            postExplosionGasRadiusOverride: null,
            postExplosionGasAmount: 255,
            applyDamageToExplosionCellsNeighbors: def.projectile.applyDamageToExplosionCellsNeighbors,
            preExplosionSpawnThingDef: def.projectile.preExplosionSpawnThingDef,
            preExplosionSpawnChance: def.projectile.preExplosionSpawnChance,
            preExplosionSpawnThingCount: def.projectile.preExplosionSpawnThingCount,
            chanceToStartFire: def.projectile.explosionChanceToStartFire,
            damageFalloff: def.projectile.explosionDamageFalloff,
            direction: direction,
            ignoredThings: null,
            affectedAngle: null,
            doSoundEffects: true,
            propagationSpeed: def.projectile.damageDef.expolosionPropagationSpeed,
            excludeRadius: 0f,
            postExplosionSpawnThingDefWater: def.projectile.postExplosionSpawnThingDefWater,
            screenShakeFactor: def.projectile.screenShakeFactor
            );

        Destroy();
    }
}
