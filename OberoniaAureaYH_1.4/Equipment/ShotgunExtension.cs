using System.Collections.Generic;
using Verse;

namespace OberoniaAurea;

public class ShotgunDirectDamage : IExposable
{
    public FloatRange distanceRange;
    public IntRange damageCount;
    public void ExposeData()
    {
        Scribe_Values.Look(ref distanceRange, "distanceRange");
        Scribe_Values.Look(ref damageCount, "damageCount");
    }
}
public class ShotgunSplashDamage : IExposable
{
    public FloatRange distanceRange;
    public float splashRadius;
    public IntRange damageCount;
    public void ExposeData()
    {
        Scribe_Values.Look(ref distanceRange, "distanceRange");
        Scribe_Values.Look(ref splashRadius, "splashRadius");
        Scribe_Values.Look(ref damageCount, "damageCount");
    }
}
public class ShotgunExtension : DefModExtension
{
    public List<ShotgunDirectDamage> shotgunDirectDamage;
    public bool splash = true;
    public List<ShotgunSplashDamage> shotgunSplashDamage;
}
