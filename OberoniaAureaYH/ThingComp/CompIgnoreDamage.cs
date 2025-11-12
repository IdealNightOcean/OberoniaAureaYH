using Verse;

namespace OberoniaAurea;

public class CompProperties_IgnoreDamage : CompProperties
{
    public DamageDef damageDef;
    public CompProperties_IgnoreDamage()
    {
        compClass = typeof(CompIgnoreDamage);
    }
}
public class CompIgnoreDamage : ThingComp
{
    private CompProperties_IgnoreDamage Props => props as CompProperties_IgnoreDamage;
    public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
    {
        absorbed = dinfo.Def == Props.damageDef;
    }
}
