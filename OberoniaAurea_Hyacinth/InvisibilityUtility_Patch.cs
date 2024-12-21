using HarmonyLib;
using Verse;

namespace OberoniaAurea_Hyacinth;

[StaticConstructorOnStartup]
[HarmonyPatch(typeof(HediffComp_Invisibility), "ForcedVisible", MethodType.Getter)]

public static class ForcedVisible_Patch
{
    [HarmonyPostfix]
    public static void PostFix(ref HediffComp_Invisibility __instance, ref bool __result)
    {
        if (!__result)
        {
            Pawn pawn = __instance.parent.pawn;
            if (pawn == null || !pawn.Spawned)
            {
                return;
            }
            GameComponent_Hyacinth HyacinthGameComp = Hyacinth_Utility.HyacinthGameComp;
            if (HyacinthGameComp != null)
            {
                __result = HyacinthGameComp.AntiStealthCheck(pawn);
            }
        }
    }
}
