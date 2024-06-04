using Verse;

namespace OberoniaAurea_Hyacinth;
public class CompProperties_RatkinHarmTurret : CompProperties
{
    public bool aiEffectiveUse;
    public float radius;
    public float RadiusSquared => radius * radius;

    public CompProperties_RatkinHarmTurret()
    {
        compClass = typeof(CompRatkinHarmTurret);
    }
}