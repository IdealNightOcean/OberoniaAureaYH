using RimWorld;
using System.Linq;
using Verse;

namespace OberoniaAurea;

public class CompProperties_ExtremelyHiTechResearchFacility : CompProperties_Facility
{
    public float normalTechResearchOffect;

    public float hiTechResearchOffect;

    public int checkInterval = 250;
    public void AdjustStatOffsets(bool hiTechResearch)
    {
        StatModifier researchSpeedFactor = statOffsets.Where(s => s.stat == StatDefOf.ResearchSpeedFactor).First();
        if (researchSpeedFactor is not null)
        {
            researchSpeedFactor.value = hiTechResearch ? hiTechResearchOffect : normalTechResearchOffect;
        }
    }

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

    public override void CompTick()
    {

        base.CompTick();
        ticksRemaining--;
        if (ticksRemaining < 0)
        {
            Props.AdjustStatOffsets(HiTechResearch);
            ticksRemaining = Props.checkInterval;
        }
    }
    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref ticksRemaining, "ticksRemaining", 0);
    }
}