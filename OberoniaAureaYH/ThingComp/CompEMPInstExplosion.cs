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
        GenExplosion.DoExplosion(parent.Position, parent.Map, Props.explosiveRadius, Props.explosiveDamageType, parent, Props.damageAmountBase, Props.armorPenetrationBase, Props.explosionSound, parent.def, null, null, Props.postExplosionSpawnThingDef, Props.postExplosionSpawnChance, Props.postExplosionSpawnThingCount, Props.postExplosionGasType, Props.applyDamageToExplosionCellsNeighbors, Props.preExplosionSpawnThingDef, Props.preExplosionSpawnChance, Props.preExplosionSpawnThingCount, Props.chanceToStartFire, Props.damageFalloff, null, [parent]);
    }
    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref disabled, "disabled", defaultValue: false);
    }
}
