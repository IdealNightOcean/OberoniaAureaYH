using RimWorld;
using Verse;

namespace OberoniaAurea;

[DefOf]
public static class OARK_HediffDefOf
{
    [MayRequireBiotech]
    public static HediffDef OA_RK_Hediff_ColdAdaptation;
    [MayRequireBiotech]
    public static HediffDef OA_RK_Hediff_HotAdaptation;

    public static HediffDef OA_RK_SeriousInjury;

    static OARK_HediffDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(OARK_HediffDefOf));
    }
}