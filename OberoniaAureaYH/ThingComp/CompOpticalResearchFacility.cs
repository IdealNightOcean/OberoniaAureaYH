using RimWorld;
using Verse;

namespace OberoniaAurea;

public class CompProperties_OpticalResearchFacility : CompProperties_Facility
{
    public float needGlowLevel;

    public int checkInterval = 600;
    public CompProperties_OpticalResearchFacility()
    {
        compClass = typeof(CompOpticalResearchFacility);
    }
}

public class CompOpticalResearchFacility : CompFacility
{
    public new CompProperties_OpticalResearchFacility Props => (CompProperties_OpticalResearchFacility)props;
    protected float cachedGlow;
    protected int ticksRemaining;
    public override bool CanBeActive => cachedGlow >= Props.needGlowLevel && base.CanBeActive;

    public override void CompTickInterval(int delta)
    {
        base.CompTickInterval(delta);
        if ((ticksRemaining -= delta) < 0)
        {
            cachedGlow = parent.Map?.glowGrid.GroundGlowAt(parent.Position) ?? 0f;
            ticksRemaining = Props.checkInterval;
        }
    }


    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref ticksRemaining, nameof(ticksRemaining), 0);
        Scribe_Values.Look(ref cachedGlow, nameof(cachedGlow), 0);
    }

}