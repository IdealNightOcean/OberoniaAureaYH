using Verse;

namespace OberoniaAurea;

public class AstableExplosiveBullet : Projectile_Explosive
{
    protected override void Explode()
    {
        Map map = base.Map;
        IntVec3 position = base.Position;
        if (def.projectile.explosionEffect != null)
        {
            Effecter effecter = def.projectile.explosionEffect.Spawn();
            effecter.Trigger(new TargetInfo(position, map), new TargetInfo(position, map));
            effecter.Cleanup();
        }
        float explosionRadius = def.projectile.explosionRadius;
        DamageDef damageDef = def.projectile.damageDef;
        Thing instigator = launcher;
        int damAmount = (int)Rand.Range(DamageAmount * 0.4f, DamageAmount * 1.2f);
        float armorPenetration = ArmorPenetration;
        SoundDef soundExplode = def.projectile.soundExplode;
        ThingDef weapon = equipmentDef;
        ThingDef projectile = def;
        Thing thing = intendedTarget.Thing;
        ThingDef postExplosionSpawnThingDef = def.projectile.postExplosionSpawnThingDef;
        ThingDef postExplosionSpawnThingDefWater = def.projectile.postExplosionSpawnThingDefWater;
        float postExplosionSpawnChance = def.projectile.postExplosionSpawnChance;
        int postExplosionSpawnThingCount = def.projectile.postExplosionSpawnThingCount;
        GasType? postExplosionGasType = def.projectile.postExplosionGasType;
        ThingDef preExplosionSpawnThingDef = def.projectile.preExplosionSpawnThingDef;
        float preExplosionSpawnChance = def.projectile.preExplosionSpawnChance;
        int preExplosionSpawnThingCount = def.projectile.preExplosionSpawnThingCount;
        GenExplosion.DoExplosion(position, map, explosionRadius, damageDef, instigator, damAmount, armorPenetration, soundExplode, weapon, projectile, thing, postExplosionSpawnThingDef, postExplosionSpawnChance, postExplosionSpawnThingCount, postExplosionGasType, def.projectile.applyDamageToExplosionCellsNeighbors, preExplosionSpawnThingDef, preExplosionSpawnChance, preExplosionSpawnThingCount, def.projectile.explosionChanceToStartFire, def.projectile.explosionDamageFalloff, origin.AngleToFlat(destination), null, null, doVisualEffects: true, def.projectile.damageDef.expolosionPropagationSpeed, 0f, doSoundEffects: true, postExplosionSpawnThingDefWater, def.projectile.screenShakeFactor);
        Destroy();
    }
}
