using HarmonyLib;
using RimWorld;
using Verse;

namespace OberoniaAurea;

[StaticConstructorOnStartup]
[HarmonyPatch(typeof(InteractionWorker_KindWords), "RandomSelectionWeight")]
public static class KindWords_RandomSelectionWeight_Patch
{
    [HarmonyPostfix]
    public static void Postfix(ref float __result, Pawn initiator, Pawn recipient)
    {
        if (__result == 0f || !ModsConfig.IdeologyActive)
        {
            return;
        }

        Ideo ideo = initiator.Ideo;
        if (ideo is null)
        {
            return;
        }

        float factor = GetPreceptFactor_Community(ideo);
        if (initiator.IsOnBirthday())
        {
            factor *= GetPreceptFactor_Birthday(ideo);
        }

        __result *= factor;
    }

    private static float GetPreceptFactor_Community(Ideo ideo)
    {
        if (ideo.HasPrecept(OARK_PreceptDefOf.OA_RK_Community_Utopia))
        {
            return 1.1f;
        }
        if (ideo.HasPrecept(OARK_PreceptDefOf.OA_RK_Community_Turbulent))
        {
            return 0.75f;
        }
        return 1f;
    }

    private static float GetPreceptFactor_Birthday(Ideo ideo)
    {
        if (ideo.HasPrecept(OARK_PreceptDefOf.OARK_Birthday_Ordinary))
        {
            return 2f;
        }
        if (ideo.HasPrecept(OARK_PreceptDefOf.OARK_Birthday_Appreciate))
        {
            return 4f;
        }
        if (ideo.HasPrecept(OARK_PreceptDefOf.OARK_Birthday_Solemn))
        {
            return 8f;
        }
        return 1f;
    }
}