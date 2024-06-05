using System.Collections.Generic;
using RimWorld;
using Verse;

namespace OberoniaAurea;

public class CompSpawnedExplosion : ThingComp
{
	private bool disabled;

	public CompProperties_Explosive Props => (CompProperties_Explosive)props;

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		base.PostSpawnSetup(respawningAfterLoad);
		if (!disabled && !respawningAfterLoad)
		{
			Explosion();
		}
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref disabled, "disabled", defaultValue: false);
	}

	public void Explosion()
	{
		disabled = true;
		GenExplosion.DoExplosion(parent.Position, parent.Map, Props.explosiveRadius, Props.explosiveDamageType, (Thing)parent, Props.damageAmountBase, Props.armorPenetrationBase, Props.explosionSound, parent.def, (ThingDef)null, (Thing)null, Props.postExplosionSpawnThingDef, Props.postExplosionSpawnChance, Props.postExplosionSpawnThingCount, Props.postExplosionGasType, Props.applyDamageToExplosionCellsNeighbors, Props.preExplosionSpawnThingDef, Props.preExplosionSpawnChance, Props.preExplosionSpawnThingCount, Props.chanceToStartFire, Props.damageFalloff, (float?)null, new List<Thing> { parent }, (FloatRange?)null, true, 1f, 0f, true, (ThingDef)null, 1f);
	}
}
