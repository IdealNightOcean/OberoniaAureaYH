using RimWorld;
using Verse;

namespace OberoniaAurea;

[DefOf]
public static class OARK_HediffDefOf
{
    ///<summary>体力耗竭</summary>
    public static HediffDef OARK_Hediff_ExplorationFatigue;
    ///<summary>严重受伤</summary>
    public static HediffDef OARK_Hediff_SeriousInjury;

    [MayRequireBiotech]
    public static HediffDef OA_RK_Hediff_ColdAdaptation;
    [MayRequireBiotech]
    public static HediffDef OA_RK_Hediff_HotAdaptation;

    [MayRequireOdyssey]
    public static HediffDef OARK_Hediff_SailorAssistance;
    [MayRequireOdyssey]
    public static HediffDef OARK_Hediff_GravFieldSuppress;
    [MayRequireOdyssey]
    public static HediffDef OARK_Hediff_Referendary;

    static OARK_HediffDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(OARK_HediffDefOf));
    }
}