using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace OberoniaAurea;

public class CompChargedExplosion : ThingComp
{
	public int ChargeProgress;

	public CompPowerTrader comppower;

	public CompRefuelable compfuel;

	public CompBreakdownable compBreakdown;

	private static readonly Material filledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.5f, 0.475f, 0.1f));

	private static readonly Material emptyMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.15f, 0.15f, 0.15f));

	private static readonly Vector2 BarSize = new Vector2(1f, 0.14f);

	public CompProperties_ChargedExplosion Props => (CompProperties_ChargedExplosion)props;

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		base.PostSpawnSetup(respawningAfterLoad);
		comppower = parent.TryGetComp<CompPowerTrader>();
		compfuel = parent.TryGetComp<CompRefuelable>();
		compBreakdown = parent.TryGetComp<CompBreakdownable>();
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref ChargeProgress, "ChargeProgress", 0);
	}

	public override void CompTick()
	{
		base.CompTick();
		if (comppower != null)
		{
			if (ChargeProgress < Props.chargeTicks)
			{
				comppower.PowerOutput = 0f - comppower.Props.PowerConsumption;
			}
			else
			{
				comppower.PowerOutput = 0f - comppower.Props.idlePowerDraw;
			}
		}
		if ((comppower == null || comppower.PowerOn) && ChargeProgress < Props.chargeTicks)
		{
			ChargeProgress++;
		}
	}

	public void Explosion()
	{
		ChargeProgress = 0;
		compfuel?.ConsumeFuel(compfuel.Fuel);
		GenExplosion.DoExplosion(parent.Position, parent.Map, Props.explosiveRadius, Props.explosiveDamageType, parent, Props.damageAmountBase, Props.armorPenetrationBase, Props.explosionSound, parent.def, null, null, Props.postExplosionSpawnThingDef, Props.postExplosionSpawnChance, Props.postExplosionSpawnThingCount, Props.postExplosionGasType, Props.applyDamageToExplosionCellsNeighbors, Props.preExplosionSpawnThingDef, Props.preExplosionSpawnChance, Props.preExplosionSpawnThingCount, Props.chanceToStartFire, Props.damageFalloff, null, new List<Thing>(1) { parent });
        GenExplosion.DoExplosion(parent.Position, parent.Map, Props.explosiveRadius, Props.requiredDamageTypeToExplode, parent, Props.damageAmountBase, Props.armorPenetrationBase, Props.explosionSound, parent.def, null, null, Props.postExplosionSpawnThingDef, Props.postExplosionSpawnChance, Props.postExplosionSpawnThingCount, Props.postExplosionGasType, Props.applyDamageToExplosionCellsNeighbors, Props.preExplosionSpawnThingDef, Props.preExplosionSpawnChance, Props.preExplosionSpawnThingCount, Props.chanceToStartFire, Props.damageFalloff, null, new List<Thing>(1) { parent });
    }

    public override void PostDraw()
	{
		base.PostDraw();
		if (Find.Selector.IsSelected(parent) && ChargeProgress > 0 && ChargeProgress < Props.chargeTicks)
		{
			GenDraw.FillableBarRequest r = default(GenDraw.FillableBarRequest);
			r.center = parent.TrueCenter() + Vector3.up;
			r.size = BarSize;
			r.fillPercent = (float)ChargeProgress / (float)Props.chargeTicks;
			r.filledMat = filledMat;
			r.unfilledMat = emptyMat;
			r.margin = 0.15f;
			GenDraw.DrawFillableBar(r);
		}
	}

	public override IEnumerable<Gizmo> CompGetGizmosExtra()
	{
		if (parent.Faction == null || parent.Faction == Faction.OfPlayer)
		{
			Command_Action command_Action = new Command_Action
			{
				defaultLabel = Props.detonateLabel,
				icon = ContentFinder<Texture2D>.Get(Props.detonateIcon, reportFailure: false),
				defaultDesc = parent.DescriptionDetailed,
				action = delegate
				{
					Explosion();
				}
			};
			if (comppower != null && !comppower.PowerOn)
			{
				command_Action.Disable(Props.noPowerReason);
			}
			if (compfuel != null && !compfuel.IsFull)
			{
				command_Action.Disable(Props.noFuelReason);
			}
			if (ChargeProgress < Props.chargeTicks)
			{
				command_Action.Disable(Props.noChargeReason);
			}
			if (compBreakdown != null && compBreakdown.BrokenDown)
			{
				command_Action.Disable(Props.noFuelReason);
			}
			yield return command_Action;
		}
	}
}
