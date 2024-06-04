using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace OberoniaAurea;

[StaticConstructorOnStartup]
[HarmonyPatch(typeof(TradeUtility))]
public static class TradeUtility_PricePatch
{
    private static List<SpecialtyPriceFactor> cachedSpecialtyPriceFactors;
    //private static Faction cachedTradeFaction;

    [HarmonyPostfix]
    [HarmonyPatch("GetPricePlayerSell")]
    public static void GetPricePlayerSell_Postfix(ref float __result, Thing thing, TradeCurrency currency)
    {
        if (currency == TradeCurrency.Favor)
        {
            return;
        }

        float price = PriceAdjust(thing.def, __result, true);
        price = Mathf.Max(price, 0.01f);
        if (price > 99.5f)
        {
            price = Mathf.Round(price);
        }
        __result = price;
    }
    [HarmonyPostfix]
    [HarmonyPatch("GetPricePlayerBuy")]
    public static void GetPricePlayerBuy_Postfix(ref float __result, Thing thing)
    {
        float price = PriceAdjust(thing.def, __result, false);
        price = Mathf.Max(price, 0.5f);
        if (price > 99.5f)
        {
            price = Mathf.Round(price);
        }
        __result = price;
    }
    private static float PriceAdjust(ThingDef thingDef, float prePrice, bool sell)
    {
        if (!TradeSession.Active)
        {
            return prePrice;
        }
        cachedSpecialtyPriceFactors = thingDef.GetModExtension<SpecialtyPriceExtension>()?.SpecialtyPriceFactors;
        if (cachedSpecialtyPriceFactors.NullOrEmpty())
        {
            return prePrice;
        }

        FactionDef tradeFaction = TradeSession.trader.Faction?.def;
        if (tradeFaction == null)
        {
            return prePrice;
        }

        float priceFactor = 1f;
        foreach (SpecialtyPriceFactor specialtyPriceFactor in cachedSpecialtyPriceFactors)
        {
            if (tradeFaction == specialtyPriceFactor.factionDef)
            {
                priceFactor = sell ? specialtyPriceFactor.sellPriceFactor : specialtyPriceFactor.buyPriceFactor;
                break;
            }
        }
        return prePrice * priceFactor;

    }

    /*
    private static float PriceAdjust(ThingDef thingDef, float prePrice, bool sell)
    {
        if (!TradeSession.Active)
        {
            return prePrice;
        }
        Faction tradeFaction = TradeSession.trader.Faction;
        if (tradeFaction == null)
        {
            return prePrice;
        }
        if (tradeFaction == cachedTradeFaction)
        {
            cachedSpecialtyPriceFactors ??= cachedTradeFaction.def.GetModExtension<SpecialtyPriceExtension>()?.SpecialtyPriceFactors;
        }
        else
        {
            cachedTradeFaction = tradeFaction;
            cachedSpecialtyPriceFactors = tradeFaction.def.GetModExtension<SpecialtyPriceExtension>()?.SpecialtyPriceFactors;
        }
        if (cachedSpecialtyPriceFactors.NullOrEmpty())
        {
            return prePrice;
        }
        float priceFactor = 1f;
        foreach (SpecialtyPriceFactor specialtyPriceFactor in cachedSpecialtyPriceFactors)
        {
            if (thingDef == specialtyPriceFactor.thingDef)
            {
                priceFactor = sell ? specialtyPriceFactor.sellPriceFactor : specialtyPriceFactor.buyPriceFactor;
                break;
            }
        }
        return prePrice * priceFactor;

    */
}

public class SpecialtyPriceExtension : DefModExtension
{
    public List<SpecialtyPriceFactor> SpecialtyPriceFactors;
}

public class SpecialtyPriceFactor : IExposable
{
    public FactionDef factionDef;
    public float sellPriceFactor = 1f;
    public float buyPriceFactor = 1f;
    public void ExposeData()
    {
        Scribe_Defs.Look(ref factionDef, "factionDef");
        Scribe_Values.Look(ref sellPriceFactor, "sellPriceFactor", 1f);
        Scribe_Values.Look(ref buyPriceFactor, "buyPriceFactor", 1f);
    }
}