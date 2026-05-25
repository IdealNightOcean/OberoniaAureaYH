using RimWorld;
using Verse;

namespace OberoniaAurea;

public class CompProperties_ExtremelyHiTechResearchFacility : CompProperties_Facility
{
    public float normalTechResearchOffect;

    public float hiTechResearchOffect;

    public int checkInterval = 250;

    public CompProperties_ExtremelyHiTechResearchFacility()
    {
        compClass = typeof(CompExtremelyHiTechResearchFacility);
    }
}


[StaticConstructorOnStartup]
public class CompExtremelyHiTechResearchFacility : CompFacility
{
    public new CompProperties_ExtremelyHiTechResearchFacility Props => (CompProperties_ExtremelyHiTechResearchFacility)props;

    protected int ticksRemaining;

    protected static bool HiTechResearch
    {
        get
        {
            ResearchProjectDef currentProj = Find.ResearchManager.GetProject();
            if (currentProj is not null)
            {
                if (currentProj.techLevel == TechLevel.Spacer || currentProj.techLevel == TechLevel.Ultra)
                {
                    return true;
                }
            }
            return false;
        }
    }

    public override void CompTickInterval(int delta)
    {
        base.CompTickInterval(delta);
        if ((ticksRemaining -= delta) < 0)
        {
            AdjustStatOffsets(HiTechResearch);
            ticksRemaining = Props.checkInterval;
        }
    }

    public void AdjustStatOffsets(bool hiTechResearch)
    {
        if (Props.statOffsets.NullOrEmpty())
            return;

        StatModifier researchSpeedModifier = null;
        foreach (StatModifier mofifier in Props.statOffsets)
        {
            if (mofifier.stat == StatDefOf.ResearchSpeedFactor)
            {
                researchSpeedModifier = mofifier;
                break;
            }
        }

        if (researchSpeedModifier is not null)
        {
            researchSpeedModifier.value = hiTechResearch ? Props.hiTechResearchOffect : Props.normalTechResearchOffect;
        }
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref ticksRemaining, nameof(ticksRemaining), 0);
    }
}