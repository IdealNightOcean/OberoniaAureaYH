using RimWorld;

namespace OberoniaAurea;

[DefOf]
public static class OARK_ThoughtDefOf
{
    [MayRequireIdeology]
    public static ThoughtDef OA_RK_Atonement;

    [MayRequireIdeology]
    public static ThoughtDef OA_RK_ResponsibilityConstraints;

    ///<summary>
    ///生日祝福
    ///</summary>
    public static ThoughtDef OARK_BirthdayWishes;

    /// <summary>
    /// 公款旅游
    /// </summary>
    public static ThoughtDef OARK_TravelAtPublicCost;

    ///<summary>
    ///海盗飞船爆炸
    ///</summary>
    [MayRequireOdyssey]
    public static ThoughtDef OARK_Thought_PirateShipExploded;

    ///<summary>
    ///科学船发射
    ///</summary>
    [MayRequireOdyssey]
    public static ThoughtDef OARK_Thought_ScienceShipLaunch;

    ///<summary>
    ///重力研究协助：情书
    ///</summary>
    [MayRequireOdyssey]
    public static ThoughtDef OARK_UnlockedFile_LoveLetter;

    ///<summary>
    ///重力研究协助：删除的文件
    ///</summary>
    [MayRequireOdyssey]
    public static ThoughtDef OARK_UnlockedFile_DeletedFile;

    ///<summary>
    ///科研部友好事件 - 研究交流
    ///</summary>
    [MayRequireOdyssey]
    public static ThoughtDef OARK_GraveResearchExchange;

    static OARK_ThoughtDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(OARK_ThoughtDefOf));
    }
}
