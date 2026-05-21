using RimWorld;
using Verse;

namespace OberoniaAurea;

[DefOf]
public static class OARK_RulePackDef
{
    /// <summary>
    /// 多发会谈 - 每个派系修正Rule
    /// </summary>
    public static RulePackDef OARK_MultiPartyTalks_BaselineText;

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