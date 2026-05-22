using HarmonyLib;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using Verse;

namespace OberoniaAurea;

[StaticConstructorOnStartup]
[HarmonyPatch(typeof(Settlement))]
public static class Settlement_ArrivalAction_Patch
{
    [HarmonyPatch(nameof(Settlement.GetFloatMenuOptions))]
    [HarmonyPostfix]
    public static IEnumerable<FloatMenuOption> Caravan_Postfix(IEnumerable<FloatMenuOption> originValue, Settlement __instance, Caravan caravan)
    {
        foreach (FloatMenuOption menuOption1 in originValue)
            yield return menuOption1;

        foreach (FloatMenuOption menuOption2 in CaravanArrivalAction_InteractWithGuest.GetFloatMenuOptions(caravan, __instance))
            yield return menuOption2;
    }

    [HarmonyPatch(nameof(Settlement.GetTransportersFloatMenuOptions))]
    [HarmonyPostfix]
    public static IEnumerable<FloatMenuOption> Transporters_Postfix(IEnumerable<FloatMenuOption> originValue, Settlement __instance, IEnumerable<IThingHolder> pods, Action<PlanetTile, TransportersArrivalAction> launchAction)
    {
        foreach (FloatMenuOption menuOption1 in originValue)
            yield return menuOption1;

        foreach (FloatMenuOption menuOption2 in TransportersArrivalAction_InteractWithGuest.GetFloatMenuOptions(launchAction, pods, __instance, isShuttle: false))
            yield return menuOption2;
    }

    [HarmonyPatch(nameof(Settlement.GetShuttleFloatMenuOptions))]
    [HarmonyPostfix]
    public static IEnumerable<FloatMenuOption> Shuttle_Postfix(IEnumerable<FloatMenuOption> originValue, Settlement __instance, IEnumerable<IThingHolder> pods, Action<PlanetTile, TransportersArrivalAction> launchAction)
    {
        foreach (FloatMenuOption menuOption1 in originValue)
            yield return menuOption1;

        if (TransportersArrivalAction_InteractWithGuest.CanVisit(pods, __instance).Accepted)
        {
            yield return new FloatMenuOption("OARK_VisitExtraGuest".Translate(), delegate
            {
                launchAction(__instance.Tile, new TransportersArrivalAction_InteractWithGuest(__instance, "MessageShuttleArrived"));
            });
        }
    }
}
