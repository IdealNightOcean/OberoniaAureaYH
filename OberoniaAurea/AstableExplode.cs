using System.Collections.Generic;
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
			effecter.Trigger(new TargetInfo(base.Position, map), new TargetInfo(base.Position, map), -1);
			effecter.Cleanup();
		}
		IntVec3 position = base.Position;
		Map map2 = map;
		float explosionRadius = def.projectile.explosionRadius;
		DamageDef damageDef = def.projectile.damageDef;
		Thing thing = launcher;
		float num = DamageAmount;
		int num2 = (int)Rand.Range(num * 0.4f, num * 1.2f);
		float armorPenetration = ArmorPenetration;
		SoundDef soundExplode = def.projectile.soundExplode;
		ThingDef thingDef = equipmentDef;
		ThingDef thingDef2 = def;
		Thing thing2 = intendedTarget.Thing;
		ThingDef postExplosionSpawnThingDef = def.projectile.postExplosionSpawnThingDef;
		ThingDef postExplosionSpawnThingDefWater = def.projectile.postExplosionSpawnThingDefWater;
		float postExplosionSpawnChance = def.projectile.postExplosionSpawnChance;
		int postExplosionSpawnThingCount = def.projectile.postExplosionSpawnThingCount;
		GasType? postExplosionGasType = def.projectile.postExplosionGasType;
		ThingDef preExplosionSpawnThingDef = def.projectile.preExplosionSpawnThingDef;
		float preExplosionSpawnChance = def.projectile.preExplosionSpawnChance;
		int preExplosionSpawnThingCount = def.projectile.preExplosionSpawnThingCount;
		GenExplosion.DoExplosion(position, map2, explosionRadius, damageDef, thing, num2, armorPenetration, soundExplode, thingDef, thingDef2, thing2, postExplosionSpawnThingDef, postExplosionSpawnChance, postExplosionSpawnThingCount, postExplosionGasType, def.projectile.applyDamageToExplosionCellsNeighbors, preExplosionSpawnThingDef, preExplosionSpawnChance, preExplosionSpawnThingCount, def.projectile.explosionChanceToStartFire, def.projectile.explosionDamageFalloff, (float?)origin.AngleToFlat(destination), (List<Thing>)null, (FloatRange?)null, true, def.projectile.damageDef.expolosionPropagationSpeed, 0f, true, postExplosionSpawnThingDefWater, def.projectile.screenShakeFactor);
	}
}
