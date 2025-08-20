using RimWorld;
using Verse;

namespace OberoniaAurea;

[DefOf]
public static class OARK_RulePackDef
{
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