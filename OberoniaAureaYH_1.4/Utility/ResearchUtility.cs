using System.Collections.Generic;
using Verse;

namespace OberoniaAurea;

public static class ResearchUtility
{
    public static Dictionary<ResearchProjectDef, float> GetProjectProgress()
    {
        return ReflectionUtility.GetFieldValue<Dictionary<ResearchProjectDef, float>>(Find.ResearchManager, "progress", null);
    }

    public static void SetProjectProgress(Dictionary<ResearchProjectDef, float> progress)
    {
        if (progress.NullOrEmpty())
        {
            return;
        }
        ReflectionUtility.SetFieldValue(Find.ResearchManager, "progress", progress);
    }
}
