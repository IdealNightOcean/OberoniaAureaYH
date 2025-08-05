using RimWorld;
using Verse;

namespace OberoniaAurea;

[DefOf]
public static class OARK_HediffDefOf
{
    public static HediffDef OARK_Hediff_ExplorationFatigue; //体力耗竭
    public static HediffDef OARK_Hediff_SeriousInjury; //严重受伤

    [MayRequireBiotech]
    public static HediffDef OA_RK_Hediff_ColdAdaptation;
    [MayRequireBiotech]
    public static HediffDef OA_RK_Hediff_HotAdaptation;

    [MayRequireOdyssey]
    public static HediffDef OARK_Hediff_SailorAssistance;
    [MayRequireOdyssey]
    public static HediffDef OARK_Hediff_GravFieldSuppress;

    static OARK_HediffDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(OARK_HediffDefOf));
    }
}