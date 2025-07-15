using RimWorld;

namespace OberoniaAurea;

[DefOf]
public static class OARK_HistoryEventDefOf
{
    public static HistoryEventDef OA_ResearchSummit;
    public static HistoryEventDef OA_AttackResearcherCamp;

    public static HistoryEventDef OA_DiplomaticSummit_LeaveHalfway;
    public static HistoryEventDef OA_DiplomaticSummit_Disaster;
    public static HistoryEventDef OA_DiplomaticSummit_Flounder;
    public static HistoryEventDef OA_DiplomaticSummit_Triumph;

    public static HistoryEventDef OA_PsychicSlaughter;

    static OARK_HistoryEventDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(OARK_HistoryEventDefOf));
    }
}
