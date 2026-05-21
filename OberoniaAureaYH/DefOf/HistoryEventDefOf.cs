using RimWorld;

namespace OberoniaAurea;

[DefOf]
public static class OARK_HistoryEventDefOf
{
    /// <summary>
    /// 外交交互
    /// </summary>
    public static HistoryEventDef OARK_DiplomacyInteraction;

    /// <summary>
    /// 帮助勘探队
    /// </summary>
    public static HistoryEventDef OARK_ProspectingTeam;

    /// <summary>
    /// 中立的和谈
    /// </summary>
    public static HistoryEventDef OARK_PeaceTalksNormal;

    /// <summary>
    /// 科研峰会
    /// </summary>
    public static HistoryEventDef OA_ResearchSummit;
    /// <summary>
    /// 袭击了参与峰会的人员
    /// </summary>
    public static HistoryEventDef OA_AttackResearcherCamp;

    /// <summary>
    /// 利益谈判 - 中途离开
    /// </summary>
    public static HistoryEventDef OA_DiplomaticSummit_LeaveHalfway;
    /// <summary>
    /// 利益谈判 - 惨痛失败
    /// </summary>
    public static HistoryEventDef OA_DiplomaticSummit_Disaster;
    /// <summary>
    /// 利益谈判 - 微小失败
    /// </summary>
    public static HistoryEventDef OA_DiplomaticSummit_Flounder;
    /// <summary>
    /// 利益谈判 - 辉煌成功
    /// </summary>
    public static HistoryEventDef OA_DiplomaticSummit_Triumph;

    /// <summary>
    /// 残忍的心灵宰杀
    /// </summary>
    public static HistoryEventDef OA_PsychicSlaughter;

    /// <summary>
    /// 
    /// </summary>
    [MayRequireOdyssey]
    public static HistoryEventDef OARK_CrashedGravshipHelp;

    static OARK_HistoryEventDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(OARK_HistoryEventDefOf));
    }
}
