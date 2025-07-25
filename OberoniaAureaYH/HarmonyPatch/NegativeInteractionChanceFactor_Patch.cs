using HarmonyLib;
using RimWorld;
using Verse;

namespace OberoniaAurea;

[StaticConstructorOnStartup]
[HarmonyPatch(typeof(NegativeInteractionUtility), "NegativeInteractionChanceFactor")]
public static class NegativeInteractionChanceFactor_Patch
{
    [HarmonyPostfix]
    public static void Postfix(ref float __result, Pawn initiator, Pawn recipient)
    {
        if (__result == 0f)
        {
            return;
        }
        if (initiator.IsSlave && !recipient.IsSlave)
        {
            return;
        }
        Ideo ideo = initiator.Ideo;
        if (ideo is null)
        {
            return;
        }
        float factor = 1f;
        if (ideo.HasPrecept(OARK_PreceptDefOf.OA_RK_Community_Utopia))
        {
            factor = 0.5f;
        }
        else if (ideo.HasPrecept(OARK_PreceptDefOf.OA_RK_Community_Harmonious))
        {
            factor = 0.75f;
        }
        else if (ideo.HasPrecept(OARK_PreceptDefOf.OA_RK_Community_Turbulent))
        {
            factor = 1.25f;
        }

        __result *= factor;
    }
}
