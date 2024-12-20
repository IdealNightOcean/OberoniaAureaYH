using UnityEngine;
using Verse;

namespace OberoniaAurea;
public class CompProperties_ExtraAllDamageRelief : CompProperties
{
    public float reliefFactor;
    public float DamageFactor => Mathf.Clamp(1f - reliefFactor, 0f, 1f);
    public CompProperties_ExtraAllDamageRelief()
    {
        compClass = typeof(CompExtraAllDamageRelief);
    }
}
public class CompExtraAllDamageRelief : ThingComp
{
    public CompProperties_ExtraAllDamageRelief Props => props as CompProperties_ExtraAllDamageRelief;
    public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
    {
        base.PostPreApplyDamage(ref dinfo, out absorbed);
        float damageAmount = dinfo.Amount * Props.DamageFactor;
        dinfo.SetAmount(damageAmount);
    }
}
