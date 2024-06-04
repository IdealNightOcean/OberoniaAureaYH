using HarmonyLib;
using RimWorld;
using Verse;

namespace OberoniaAurea;

[StaticConstructorOnStartup]
[HarmonyPatch(typeof(ShortCircuitUtility))]
public static class ShortCircuitUtility_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch("DoShortCircuit")]
    public static bool DoShortCircuit_Prefix(Building culprit)
    {
        PowerNet powerNet = culprit.PowerComp.PowerNet;
        MapComponent_OberoniaAurea mc_OA = culprit.Map.GetComponent<MapComponent_OberoniaAurea>();
        if (mc_OA == null)
        {
            return true;
        }
        if (mc_OA.AverageCircuitStability(powerNet) > 0.2f)
        {
            return false;
        }
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch("TryShortCircuitInRain")]
    public static bool TryShortCircuitInRain_Prefix(ref bool __result, Thing thing)
    {
        PowerNet powerNet = thing.TryGetComp<CompPowerTrader>()?.PowerNet;
        if (powerNet == null)
        {
            return true;
        }
        MapComponent_OberoniaAurea mc_OA = thing.Map?.GetComponent<MapComponent_OberoniaAurea>();
        if (mc_OA == null)
        {
            return true;
        }
        if (mc_OA.AverageCircuitStability(powerNet) > 0.2f)
        {
            __result = false;
            return false;
        }
        return true;
    }
}
