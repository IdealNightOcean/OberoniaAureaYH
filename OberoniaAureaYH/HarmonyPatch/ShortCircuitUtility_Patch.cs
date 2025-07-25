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
        MapComponent_OberoniaAurea oaMapComp = culprit.Map.GetOAMapComp();
        if (oaMapComp is null)
        {
            return true;
        }
        if (oaMapComp.AverageCircuitStability(powerNet) > 0.2f)
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
        if (powerNet is null)
        {
            return true;
        }
        MapComponent_OberoniaAurea oaMapComp = thing.Map.GetOAMapComp();
        if (oaMapComp is null)
        {
            return true;
        }
        if (oaMapComp.AverageCircuitStability(powerNet) > 0.2f)
        {
            __result = false;
            return false;
        }
        return true;
    }
}
