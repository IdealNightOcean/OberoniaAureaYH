using HarmonyLib;
using RimWorld;
using Verse;

namespace OberoniaAurea;

[HarmonyPatch(typeof(Pawn_InteractionsTracker), "SocialFightChance")]
public static class Patch_Fight
{
    public static void Postfix(ref float __result, Pawn initiator)
	{
		Ideo ideo = initiator.Ideo;
		if (ideo != null)
		{
			if (ideo.HasPrecept(OADefOf.OA_RK_Community_Utopia))
			{
				__result *= 0.2f;
			}
			if (ideo.HasPrecept(OADefOf.OA_RK_Community_Harmonious))
			{
				__result *= 0.5f;
			}
			if (ideo.HasPrecept(OADefOf.OA_RK_Community_Turbulent))
			{
				__result *= 1.25f;
			}
		}
	}
}
