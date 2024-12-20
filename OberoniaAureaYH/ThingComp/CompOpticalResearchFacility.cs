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
    public override bool CanBeActive
    {
        get
        {
            if (!base.CanBeActive)
            {
                return false;
            }
            else
            {
                return cachedGlow >= Props.needGlowLevel;
            }
        }
    }
    public override void CompTick()
    {
        base.CompTick();
        ticksRemaining--;
        if (ticksRemaining < 0)
        {
            cachedGlow = parent.Map.glowGrid.GroundGlowAt(parent.Position);
            ticksRemaining = Props.checkInterval;
        }
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref ticksRemaining, "ticksRemaining", 0);
        Scribe_Values.Look(ref cachedGlow, "cachedGlow", 0);
    }

}