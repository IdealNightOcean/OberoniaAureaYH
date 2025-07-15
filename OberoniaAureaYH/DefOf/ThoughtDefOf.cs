using RimWorld;

namespace OberoniaAurea;

[DefOf]
public static class OARK_ThoughtDefOf
{
    [MayRequireIdeology]
    public static ThoughtDef OA_RK_Atonement;
    [MayRequireIdeology]
    public static ThoughtDef OA_RK_ResponsibilityConstraints;

    [MayRequireOdyssey]
    public static ThoughtDef OARK_Thought_PirateShipExploded; //海盗飞船爆炸
    [MayRequireOdyssey]
    public static ThoughtDef OARK_Thought_ScienceShipLaunch; //科学船发射
    [MayRequireOdyssey]
    public static ThoughtDef OARK_UnlockedFile_LoveLetter; // 重力研究协助：情书
    [MayRequireOdyssey]
    public static ThoughtDef OARK_UnlockedFile_DeletedFile; // 重力研究协助：删除的文件
    [MayRequireOdyssey]
    public static ThoughtDef OARK_GraveResearchExchange; // 友好事件 - 研究交流

    static OARK_ThoughtDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(OARK_ThoughtDefOf));
    }
}
