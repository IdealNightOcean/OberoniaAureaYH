using Verse;

namespace OberoniaAurea_Hyacinth;
public class CompProperties_AntiStealth : CompProperties
{
    public bool aiEffectiveUse;
    public float radius;
    public float RadiusSquared => radius * radius;

    public CompProperties_AntiStealth()
    {
        compClass = typeof(CompAntiStealth);
    }
}