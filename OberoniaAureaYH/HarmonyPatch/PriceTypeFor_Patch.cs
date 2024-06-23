using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea;

[StaticConstructorOnStartup]
[HarmonyPatch(typeof(TraderKindDef), "PriceTypeFor")]
public static class PriceTypeFor_Patch
{
    private static List<SpecialtyPriceFactor> cachedSpecialtyPriceFactors;

    [HarmonyPostfix]
    public static void Postfix(ref PriceType __result, ThingDef thingDef)
    {
        if (__result == PriceType.VeryCheap || !TradeSession.Active)
        {
            return;
        }
        FactionDef tradeFaction = TradeSession.trader.Faction?.def;
        if (tradeFaction == null)
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
                __result = PriceType.VeryCheap;
                return;
            }
        }
    }
}

