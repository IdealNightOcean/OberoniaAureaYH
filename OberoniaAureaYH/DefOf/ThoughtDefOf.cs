using RimWorld;

namespace OberoniaAurea;

[DefOf]
public static class OARK_ThoughtDefOf
{
    [MayRequireIdeology]
    public static ThoughtDef OA_RK_Atonement;
    [MayRequireIdeology]
    public static ThoughtDef OA_RK_ResponsibilityConstraints;

    ///<summary>生日祝福</summary>
    public static ThoughtDef OARK_BirthdayWishes;

    [MayRequireOdyssey]
    ///<summary>海盗飞船爆炸</summary>
    public static ThoughtDef OARK_Thought_PirateShipExploded;
    [MayRequireOdyssey]
    ///<summary>科学船发射</summary>
    public static ThoughtDef OARK_Thought_ScienceShipLaunch;
    [MayRequireOdyssey]
    ///<summary>重力研究协助：情书</summary>
    public static ThoughtDef OARK_UnlockedFile_LoveLetter;
    [MayRequireOdyssey]
    ///<summary>重力研究协助：删除的文件</summary>
    public static ThoughtDef OARK_UnlockedFile_DeletedFile;
    [MayRequireOdyssey]
    ///<summary>友好事件 - 研究交流</summary>
    public static ThoughtDef OARK_GraveResearchExchange;

    static OARK_ThoughtDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(OARK_ThoughtDefOf));
    }
}
