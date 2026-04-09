using RimWorld;

namespace OberoniaAurea;

[DefOf]
public static class OARK_PreceptDefOf
{
    [MayRequireIdeology]
    public static PreceptDef OA_RK_MentalBreakProbability_Atonement;
    [MayRequireIdeology]
    public static PreceptDef OA_RK_MentalBreakProbability_Low;
    [MayRequireIdeology]
    public static PreceptDef OA_RK_Community_Turbulent;
    [MayRequireIdeology]
    public static PreceptDef OA_RK_Community_Common;
    [MayRequireIdeology]
    public static PreceptDef OA_RK_Community_Harmonious;
    [MayRequireIdeology]
    public static PreceptDef OA_RK_Community_Utopia;
    [MayRequireIdeology]
    public static PreceptDef OA_RK_LeaderAttitude_Respect;
    [MayRequireIdeology]
    public static PreceptDef OA_RK_LeaderAttitude_Worship;

    /// <summary>
    /// 诞生日（恐惧）
    /// </summary>
    [MayRequireIdeology]
    public static PreceptDef OARK_Birthday_Fear;
    /// <summary>
    /// 诞生日（漠视）
    /// </summary>
    [MayRequireIdeology]
    public static PreceptDef OARK_Birthday_Ignore;
    /// <summary>
    /// 诞生日（平淡）
    /// </summary>
    [MayRequireIdeology]
    public static PreceptDef OARK_Birthday_Ordinary;
    /// <summary>
    /// 诞生日（重视）
    /// </summary>
    [MayRequireIdeology]
    public static PreceptDef OARK_Birthday_Appreciate;
    /// <summary>
    /// 诞生日（庄重）
    /// </summary>
    [MayRequireIdeology]
    public static PreceptDef OARK_Birthday_Solemn;

    static OARK_PreceptDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(OARK_PreceptDefOf));
    }
}