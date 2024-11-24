using OberoniaAurea_Frame;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea;

public static class ResearchUtility
{
    public static Dictionary<ResearchProjectDef, float> GetProjectProgress()
    {
        return OAFrame_ReflectionUtility.GetFieldValue<Dictionary<ResearchProjectDef, float>>(Find.ResearchManager, "progress", null);
    }

    public static void SetProjectProgress(Dictionary<ResearchProjectDef, float> progress)
    {
        if (progress.NullOrEmpty())
        {
            return;
        }
        OAFrame_ReflectionUtility.SetFieldValue(Find.ResearchManager, "progress", progress);
    }
    public static Dictionary<ResearchProjectDef, float> GetAnomalyKnowledges()
    {
        return OAFrame_ReflectionUtility.GetFieldValue<Dictionary<ResearchProjectDef, float>>(Find.ResearchManager, "anomalyKnowledge", null);
    }
    public static void SetAnomalyKnowledges(Dictionary<ResearchProjectDef, float> anomalyKnowledge)
    {
        if (anomalyKnowledge.NullOrEmpty())
        {
            return;
        }
        OAFrame_ReflectionUtility.SetFieldValue(Find.ResearchManager, "anomalyKnowledge", anomalyKnowledge);
    }
}
