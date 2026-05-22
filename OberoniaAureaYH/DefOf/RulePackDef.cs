using RimWorld;
using Verse;

namespace OberoniaAurea;

[DefOf]
public static class OARK_RulePackDef
{
    /// <summary>
    /// 多方会谈 - 每个派系的修正明细
    /// </summary>
    public static RulePackDef OARK_MultiPartyTalks_BaselineText;

    /// <summary>
    /// 多方会谈 - 议题详情文本
    /// </summary>
    public static RulePackDef OARK_MultiPartyTalks_TopicInfo;

    /// <summary>
    /// 多方会谈 - 议题的每通用奖励明细
    /// </summary>
    public static RulePackDef OARK_MultiPartyTalks_TopicOutcome;

    [MayRequireOdyssey]
    public static RulePackDef OARK_RulePack_ScienceShipName;
    [MayRequireOdyssey]
    public static RulePackDef OARK_RulePack_ScienceShipQuiz;
    [MayRequireOdyssey]
    public static RulePackDef OARK_RulePack_SalutationText;
    [MayRequireOdyssey]
    public static RulePackDef OARK_RulePack_ProgressText;
    [MayRequireOdyssey]
    public static RulePackDef OARK_RulePack_ProgressText_EM;
    [MayRequireOdyssey]
    public static RulePackDef OARK_RulePack_AccountInquiryText;
    [MayRequireOdyssey]
    public static RulePackDef OARK_RulePack_ReferendaryHackText;

    static OARK_RulePackDef()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(OARK_RulePackDef));
    }
}