using HarmonyLib;
using RimWorld;
using Verse;

namespace OberoniaAurea;

[HarmonyPatch(typeof(Pawn_InteractionsTracker), "SocialFightChance")]
public static class SocialFightChance_Patch
{
    public static void Postfix(ref float __result, Pawn initiator)
    {
        if (__result == 0f)
        {
            return;
        }
        Ideo ideo = initiator.Ideo;
        if (ideo == null)
        {
            return;
        }
        float factor = 1f;
        if (ideo.HasPrecept(OARatkin_PreceptDefOf.OA_RK_Community_Utopia))
        {
            factor = 0.2f;
        }
        else if (ideo.HasPrecept(OARatkin_PreceptDefOf.OA_RK_Community_Harmonious))
        {
            factor = 0.5f;
        }
        else if (ideo.HasPrecept(OARatkin_PreceptDefOf.OA_RK_Community_Turbulent))
        {
            factor = 1.25f;
        }
        __result *= factor;
    }
}


