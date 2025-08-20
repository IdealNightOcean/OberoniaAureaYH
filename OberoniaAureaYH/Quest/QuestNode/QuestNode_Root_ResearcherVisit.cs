using OberoniaAurea_Frame;
using RimWorld;
using Verse;

namespace OberoniaAurea;
public class QuestNode_Root_ResearcherVisit : QuestNode_Root_RefugeeBase
{
    private static readonly IntRange IntellectualSkill = new(8, 18);

    protected override void InitQuestParameter()
    {
        questParameter = new QuestParameter()
        {
            allowAssaultColony = true,
            allowBadThought = false,
            allowLeave = false,
            LodgerCount = Rand.RangeInclusive(2, 3),
            ChildCount = 0,
            questDurationTicks = 240000,
            arrivalDelayTicks = 120000,
            goodwillFailure = 0,
            goodwillSuccess = 0,

            fixedPawnKind = PawnKindDefOf.Empire_Common_Lodger
        };
    }

    protected override void PostPawnGenerated(Pawn pawn)
    {
        SkillRecord intellectual = pawn.skills?.GetSkill(SkillDefOf.Intellectual);
        if (intellectual is not null && intellectual.Level < 8)
        {
            intellectual.Level = IntellectualSkill.RandomInRange;
        }
    }
}
