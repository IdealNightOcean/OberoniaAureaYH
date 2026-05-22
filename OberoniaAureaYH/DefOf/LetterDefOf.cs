using RimWorld;
using Verse;

namespace OberoniaAurea;

[DefOf]
public static class OARK_LetterDefOf
{
    public static LetterDef OA_RK_AcceptJoinerViewInfo;

    /// <summary>
    /// 多方会谈 - 议题
    /// </summary>
    public static LetterDef OARK_MultiPartyTalksTopic;

    [MayRequireOdyssey]
    public static LetterDef OARK_BrokenPilotConsoleLetter;

    [MayRequireOdyssey]
    public static LetterDef OARK_UnlockedFileLetter;

    [MayRequireOdyssey]
    public static LetterDef OARK_UnlockedFilePeepLetter;

    [MayRequireOdyssey]
    public static LetterDef OARK_ScienceDepartmentFriendlyLetter;

    [MayRequireOdyssey]
    public static LetterDef OARK_EMReviewLetter;

    [MayRequireOdyssey]
    public static LetterDef OARK_EMReview_RejectLetter;

    static OARK_LetterDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(OARK_LetterDefOf));
    }
}