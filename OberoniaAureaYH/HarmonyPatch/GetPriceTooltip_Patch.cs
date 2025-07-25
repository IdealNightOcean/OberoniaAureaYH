using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea;

[StaticConstructorOnStartup]
[HarmonyPatch(typeof(Tradeable), "GetPriceTooltip")]
public static class GetPriceTooltip_Patch
{
    private static List<SpecialtyPriceFactor> cachedSpecialtyPriceFactors;

    [HarmonyPostfix]
    public static void Postfix(ref Tradeable __instance, ref string __result, TradeAction action)
    {
        if (action == TradeAction.None || !TradeSession.Active)
        {
            return;
        }
        ThingDef thingDef = __instance.ThingDef;
        if (thingDef is null)
        {
            return;
        }
        FactionDef tradeFaction = TradeSession.trader.Faction?.def;
        if (tradeFaction is null)
        {
            return;
        }

        cachedSpecialtyPriceFactors = thingDef.GetModExtension<SpecialtyPriceExtension>()?.SpecialtyPriceFactors;
        if (cachedSpecialtyPriceFactors.NullOrEmpty())
        {
            return;
        }
        foreach (SpecialtyPriceFactor specialtyPriceFactor in cachedSpecialtyPriceFactors)
        {
            if (tradeFaction == specialtyPriceFactor.factionDef)
            {
                float factor = action == TradeAction.PlayerSells ? specialtyPriceFactor.sellPriceFactor : specialtyPriceFactor.buyPriceFactor;
                __result += "\n" + "OA_SpecialtyFactor".Translate() + ": x " + factor.ToStringPercent();
                return;
            }
        }
    }
}
