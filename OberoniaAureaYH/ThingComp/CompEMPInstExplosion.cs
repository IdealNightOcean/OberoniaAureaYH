using RimWorld;
using Verse;

namespace OberoniaAurea;

public class CompEMPInstExplosion : ThingComp
{
    protected bool disabled;
    public CompProperties_Explosive Props => (CompProperties_Explosive)props;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        if (!disabled && !respawningAfterLoad)
        {
            Explosion();
        }
    }
    public void Explosion()
    {
        disabled = true;
        GenExplosion.DoExplosion(center: parent.Position,
            map: parent.Map,
            radius: Props.explosiveRadius,
            damType: Props.explosiveDamageType,
            instigator: parent,
            damAmount: Props.damageAmountBase,
            armorPenetration: Props.armorPenetrationBase,
            explosionSound: Props.explosionSound,
            weapon: parent.def,
            projectile: null,
            intendedTarget: null,
            postExplosionSpawnThingDef: Props.postExplosionSpawnThingDef,
            postExplosionSpawnChance: Props.postExplosionSpawnChance,
            postExplosionSpawnThingCount: Props.postExplosionSpawnThingCount,
            postExplosionGasType: Props.postExplosionGasType,
            postExplosionGasRadiusOverride: null,
            postExplosionGasAmount: 255,
            applyDamageToExplosionCellsNeighbors: Props.applyDamageToExplosionCellsNeighbors,
            preExplosionSpawnThingDef: Props.preExplosionSpawnThingDef,
            preExplosionSpawnChance: Props.preExplosionSpawnChance,
            preExplosionSpawnThingCount: Props.preExplosionSpawnThingCount,
            chanceToStartFire: Props.chanceToStartFire,
            damageFalloff: Props.damageFalloff,
            direction: null,
            ignoredThings: [parent]);
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref disabled, "disabled", defaultValue: false);
    }
}
