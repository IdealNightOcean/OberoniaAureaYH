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

    static OARK_PreceptDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(OARK_PreceptDefOf));
    }
}