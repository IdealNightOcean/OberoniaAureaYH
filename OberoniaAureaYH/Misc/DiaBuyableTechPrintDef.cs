﻿using RimWorld;
using Verse;

namespace OberoniaAurea;

public class DiaBuyableTechPrintDef : Def
{
    public ResearchProjectDef researchProject;
    public int price;
    public int allianceDuration = -1;
    public RoyalTitleDef royalTitleDef;

    public ThingDef TechPrintDef => researchProject?.Techprint;
    public bool NeedAllianceDuration => allianceDuration > 0;
    public bool NeedRoyalTitle => royalTitleDef is not null;

    public override void ResolveReferences()
    {
        base.ResolveReferences();
        if (researchProject == null)
        {
            Log.Error($"{this.defName} has no tech research project.");
        }
        else if (TechPrintDef == null)
        {
            Log.Error($"Research Project {researchProject.label} has no tech print.");
        }

        if (price < 0)
        {
            price = 0;
        }
    }

}
