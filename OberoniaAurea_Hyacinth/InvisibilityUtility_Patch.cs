using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea_Hyacinth;

[StaticConstructorOnStartup]
[HarmonyPatch(typeof(InvisibilityUtility))]
public static class InvisibilityUtility_Patch
{
	[HarmonyPostfix]
    [HarmonyPatch("IsPsychologicallyInvisible")]
    public static void IsPsychologicallyInvisible_PostFix(ref bool __result, Pawn pawn)
	{
		if (__result)
		{
            GameComponent_Hyacinth gameComponent_Hyacinth = Current.Game.GetComponent<GameComponent_Hyacinth>();
			if (gameComponent_Hyacinth != null)
			{
				__result = gameComponent_Hyacinth.AntiStealthCheck(pawn.Map, pawn);
			}
		}
	}

    [HarmonyPostfix]
    [HarmonyPatch("IsHiddenFromPlayer")]
    public static void IsHiddenFromPlayer_Postfix(ref bool __result, Pawn pawn)
    {
        if (__result)
        {
            GameComponent_Hyacinth gameComponent_Hyacinth = Current.Game.GetComponent<GameComponent_Hyacinth>();
            if (gameComponent_Hyacinth != null)
            {
                __result = gameComponent_Hyacinth.AntiStealthCheck(pawn.Map, pawn);
            }
        }
    }
    [HarmonyPostfix]
    [HarmonyPatch("GetAlpha")]
    public static void GetAlpha(ref float __result, Pawn pawn)
    {
        if (__result != 1f)
        {
            GameComponent_Hyacinth gameComponent_Hyacinth = Current.Game.GetComponent<GameComponent_Hyacinth>();
            if (!gameComponent_Hyacinth.AntiStealthCheck(pawn.Map, pawn))
            {
                __result = 1f;
            }
        }
    }

}
