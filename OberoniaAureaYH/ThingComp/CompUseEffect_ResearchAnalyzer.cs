using RimWorld;
using Verse;

namespace OberoniaAurea;

//科研分析仪：立刻增加当前研究指定研究点数
public class CompProperties_UseEffect_ResearchAnalyzer : CompProperties_UseEffect
{
    public float researchAmount = 1000f;

    public CompProperties_UseEffect_ResearchAnalyzer()
    {
        compClass = typeof(CompUseEffect_ResearchAnalyzer);
    }
}
public class CompUseEffect_ResearchAnalyzer : CompUseEffect
{
    public CompProperties_UseEffect_ResearchAnalyzer Props => (CompProperties_UseEffect_ResearchAnalyzer)props;

    public override void DoEffect(Pawn usedBy)
    {
        base.DoEffect(usedBy);
        ResearchProjectDef curResearch = Find.ResearchManager.GetProject();
        if (curResearch != null)
        {
            Find.ResearchManager.AddProgress(curResearch, Props.researchAmount, usedBy);
            if (curResearch.IsFinished)
            {
                Messages.Message("MessageResearchProjectFinishedByItem".Translate(curResearch.LabelCap), usedBy, MessageTypeDefOf.PositiveEvent);
            }
        }
    }

    public override AcceptanceReport CanBeUsedBy(Pawn p)
    {
        if (Find.ResearchManager.GetProject() == null)
        {
            return "NoActiveResearchProjectToFinish".Translate();
        }
        return true;
    }
}