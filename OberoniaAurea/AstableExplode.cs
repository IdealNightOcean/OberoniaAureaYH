using Verse;

namespace OberoniaAurea;

public class AstableExplode : Projectile_Explosive
{
	protected override void Explode()
	{
		Map map = base.Map;
		Destroy();
		if (def.projectile.explosionEffect != null)
		{
			Effecter effecter = def.projectile.explosionEffect.Spawn();
			effecter.Trigger(new TargetInfo(base.Position, map), new TargetInfo(base.Position, map));
			effecter.Cleanup();
		}
		IntVec3 position = base.Position;
		Map map2 = map;
		float explosionRadius = def.projectile.explosionRadius;
		DamageDef damageDef = def.projectile.damageDef;
		Thing instigator = launcher;
		float num = DamageAmount;
		int damAmount = (int)Rand.Range(num * 0.4f, num * 1.2f);
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
		GenExplosion.DoExplosion(position, map2, explosionRadius, damageDef, instigator, damAmount, armorPenetration, soundExplode, weapon, projectile, thing, postExplosionSpawnThingDef, postExplosionSpawnChance, postExplosionSpawnThingCount, postExplosionGasType, def.projectile.applyDamageToExplosionCellsNeighbors, preExplosionSpawnThingDef, preExplosionSpawnChance, preExplosionSpawnThingCount, def.projectile.explosionChanceToStartFire, def.projectile.explosionDamageFalloff, origin.AngleToFlat(destination), null, null, doVisualEffects: true, def.projectile.damageDef.expolosionPropagationSpeed, 0f, doSoundEffects: true, postExplosionSpawnThingDefWater, def.projectile.screenShakeFactor);
	}
}
