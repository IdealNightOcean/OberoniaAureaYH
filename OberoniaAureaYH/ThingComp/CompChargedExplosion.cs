using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace OberoniaAurea;

public class CompProperties_ChargedExplosion : CompProperties_Explosive
{
    public int chargeTicks = 100000;

    [MustTranslate]
    public string detonateLabel = "detonate";
    [MustTranslate]
    public string noPowerReason = "no power";
    [MustTranslate]
    public string noFuelReason = "no fuel";
    [MustTranslate]
    public string cooling = "cooling";
    [MustTranslate]
    public string breakdownReason = "broken";

    public CompProperties_ChargedExplosion()
    {
        compClass = typeof(CompChargedExplosion);
    }
}

[StaticConstructorOnStartup]
public class CompChargedExplosion : ThingComp
{
    protected static readonly Texture2D DetonateIcon = ContentFinder<Texture2D>.Get("Ui/OA_RK_EMP_Jineng");

    protected static readonly Material filledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.5f, 0.475f, 0.1f));
    protected static readonly Material emptyMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.15f, 0.15f, 0.15f));
    protected static readonly Vector2 BarSize = new(1f, 0.14f);
    public CompProperties_ChargedExplosion Props => (CompProperties_ChargedExplosion)props;

    public int chargeProgress;
    public CompPowerTrader powerComp;
    public CompRefuelable refuelComp;
    public CompBreakdownable breakdownComp;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        powerComp = parent.TryGetComp<CompPowerTrader>();
        refuelComp = parent.TryGetComp<CompRefuelable>();
        breakdownComp = parent.TryGetComp<CompBreakdownable>();
    }

    public override void CompTick()
    {
        if (chargeProgress < Props.chargeTicks)
        {
            if (powerComp == null)
            {
                chargeProgress++;
            }
            else
            {
                if (powerComp.PowerOn)
                {
                    chargeProgress++;
                }
                powerComp.PowerOutput = 0f - powerComp.Props.PowerConsumption;
            }
        }
        else if (powerComp != null)
        {
            powerComp.PowerOutput = 0f - powerComp.Props.idlePowerDraw;
        }
    }

    public void Explosion()
    {
        chargeProgress = 0;
        refuelComp?.ConsumeFuel(refuelComp.Fuel);
        GenExplosion.DoExplosion(parent.Position, parent.Map, Props.explosiveRadius, Props.explosiveDamageType, parent, Props.damageAmountBase, Props.armorPenetrationBase, Props.explosionSound, parent.def, null, null, Props.postExplosionSpawnThingDef, Props.postExplosionSpawnChance, Props.postExplosionSpawnThingCount, Props.postExplosionGasType, Props.applyDamageToExplosionCellsNeighbors, Props.preExplosionSpawnThingDef, Props.preExplosionSpawnChance, Props.preExplosionSpawnThingCount, Props.chanceToStartFire, Props.damageFalloff, null, [parent]);
        GenExplosion.DoExplosion(parent.Position, parent.Map, Props.explosiveRadius, Props.requiredDamageTypeToExplode, parent, Props.damageAmountBase, Props.armorPenetrationBase, Props.explosionSound, parent.def, null, null, Props.postExplosionSpawnThingDef, Props.postExplosionSpawnChance, Props.postExplosionSpawnThingCount, Props.postExplosionGasType, Props.applyDamageToExplosionCellsNeighbors, Props.preExplosionSpawnThingDef, Props.preExplosionSpawnChance, Props.preExplosionSpawnThingCount, Props.chanceToStartFire, Props.damageFalloff, null, [parent]);
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        if (parent.Faction == null || parent.Faction.IsPlayer)
        {
            Command_Action command_Action = new()
            {
                defaultLabel = Props.detonateLabel,
                icon = DetonateIcon,
                defaultDesc = parent.DescriptionDetailed,
                action = Explosion
            };

            if (powerComp != null && !powerComp.PowerOn)
            {
                command_Action.Disable(Props.noPowerReason);
            }
            else if (refuelComp != null && !refuelComp.IsFull)
            {
                command_Action.Disable(Props.noFuelReason);
            }
            else if (chargeProgress < Props.chargeTicks)
            {
                command_Action.Disable(Props.cooling);
            }
            else if (breakdownComp != null && breakdownComp.BrokenDown)
            {
                command_Action.Disable(Props.noFuelReason);
            }
            yield return command_Action;
        }
    }

    public override void PostDraw()
    {
        base.PostDraw();
        if (Find.Selector.IsSelected(parent) && chargeProgress < Props.chargeTicks)
        {
            GenDraw.FillableBarRequest r = default;
            r.center = parent.TrueCenter() + Vector3.up;
            r.size = BarSize;
            r.fillPercent = (float)chargeProgress / (float)Props.chargeTicks;
            r.filledMat = filledMat;
            r.unfilledMat = emptyMat;
            r.margin = 0.15f;
            GenDraw.DrawFillableBar(r);
        }
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref chargeProgress, "chargeProgress", 0);
    }
}
