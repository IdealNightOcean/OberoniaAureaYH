using HarmonyLib;
using RimWorld;
using Verse;

namespace OberoniaAurea;

[StaticConstructorOnStartup]
[HarmonyPatch(typeof(CompBreakdownable), "DoBreakdown")]
public static class CompBreakdownable_Patch
{
    [HarmonyPrefix]
    public static bool DoBreakdown_Prefix(ref CompBreakdownable __instance)
    {
        CompPowerTrader powerComp = __instance.parent.GetComp<CompPowerTrader>();
        if (powerComp == null)
        {
            return true;
        }
        MapComponent_OberoniaAurea oaMapComp = __instance.parent.Map.GetOAMapComp();
        if (oaMapComp == null || oaMapComp.AverageCircuitStability(powerComp.PowerNet) <= 0f)
        {
            return true;
        }
        if (Rand.Chance(0.95f))
        {
            return false;
        }
        return true;
    }
}
