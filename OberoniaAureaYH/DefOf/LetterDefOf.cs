using RimWorld;
using Verse;

namespace OberoniaAurea;

[DefOf]
public static class OARK_LetterDefOf
{
    public static LetterDef OA_RK_AcceptJoinerViewInfo;
    [MayRequireOdyssey]
    public static LetterDef OARK_BrokenPilotConsoleLetter;
    [MayRequireOdyssey]
    public static LetterDef OARK_UnlockedFileLetter;
    [MayRequireOdyssey]
    public static LetterDef OARK_UnlockedFilePeepLetter;
    [MayRequireOdyssey]
    public static LetterDef OARK_ScienceDepartmentFriendlyLetter;

    static OARK_LetterDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(OARK_LetterDefOf));
    }
}