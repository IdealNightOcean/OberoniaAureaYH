using HarmonyLib;
using RimWorld;
using Verse;

namespace OberoniaAurea;

[StaticConstructorOnStartup]
[HarmonyPatch(typeof(InteractionWorker_KindWords), "RandomSelectionWeight")]
public static class KindWords_RandomSelectionWeight_Patch
{
    [HarmonyPostfix]
    public static void Postfix(ref float __result, Pawn initiator)
    {
        if (__result == 0f)
        {
            return;
        }
        Ideo ideo = initiator.Ideo;
        if (ideo is null)
        {
            return;
        }
        float factor = 1f;
        if (ideo.HasPrecept(OARatkin_PreceptDefOf.OA_RK_Community_Utopia))
        {
            factor = 1.1f;
        }
        else if (ideo.HasPrecept(OARatkin_PreceptDefOf.OA_RK_Community_Turbulent))
        {
            factor = 0.75f;
        }
        __result *= factor;
    }
}
