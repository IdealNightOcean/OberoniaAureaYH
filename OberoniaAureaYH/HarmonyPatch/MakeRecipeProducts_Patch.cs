using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace OberoniaAurea;
//布料回收装置，返还部分主要材料
[StaticConstructorOnStartup]
[HarmonyPatch(typeof(GenRecipe), "MakeRecipeProducts")]
public static class MakeRecipeProducts_Patch
{
    [HarmonyPostfix]
    public static IEnumerable<Thing> Postfix(IEnumerable<Thing> values, Thing dominantIngredient, IBillGiver billGiver)
    {
        foreach (Thing thing in values)
        {
            yield return thing;
        }
        if (dominantIngredient is not null && billGiver is ThingWithComps WorkTable && AffectedBySpecialFacility(WorkTable))
        {
            Thing recycleThing = ThingMaker.MakeThing(dominantIngredient.def);
            recycleThing.stackCount = Mathf.Max(dominantIngredient.stackCount / 10, 1);
            yield return recycleThing;
        }
    }

    private static bool AffectedBySpecialFacility(ThingWithComps building)
    {
        CompAffectedByFacilities compABF = building?.GetComp<CompAffectedByFacilities>();
        if (compABF is null)
        {
            return false;
        }
        foreach (Thing facility in compABF.LinkedFacilitiesListForReading)
        {
            if (facility.def == OARK_ThingDefOf.OA_RK_Cloth_Processing_B)
            {
                return true;
            }
        }
        return false;
    }

}

