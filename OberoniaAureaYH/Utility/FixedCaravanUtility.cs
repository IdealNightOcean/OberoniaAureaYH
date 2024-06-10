﻿using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace OberoniaAurea;

public static class FixedCaravanUtility
{
    private static readonly List<Thing> TempInventoryItems = [];
    private static readonly List<Thing> TempAddedItems = [];
    private static readonly List<Pawn> TempPawns = [];
    public static List<Thing> AllInventoryItems(FixedCaravan fixedCaravan)
    {
        TempInventoryItems.Clear();
        List<Pawn> allPawnsForReading = fixedCaravan.PawnsListForReading;
        for (int i = 0; i < allPawnsForReading.Count; i++)
        {
            Pawn pawn = allPawnsForReading[i];
            for (int j = 0; j < pawn.inventory.innerContainer.Count; j++)
            {
                Thing item = pawn.inventory.innerContainer[j];
                TempInventoryItems.Add(item);
            }
        }
        return TempInventoryItems;
    }
    public static FixedCaravan CreateFixedCaravan(Caravan caravan, WorldObjectDef def, int initTicks = 0)
    {
        FixedCaravan fixedCaravan = (FixedCaravan)WorldObjectMaker.MakeWorldObject(def);
        fixedCaravan.curName = caravan.Name;
        fixedCaravan.Tile = caravan.Tile;
        fixedCaravan.ticksRemaining = initTicks;
        fixedCaravan.SetFaction(caravan.Faction);
        ConvertToFixedCaravan(caravan, fixedCaravan);
        return fixedCaravan;
    }
    public static void ConvertToFixedCaravan(Caravan caravan, FixedCaravan fixedCaravan)
    {
        TempPawns.Clear();
        TempPawns.AddRange(caravan.PawnsListForReading);
        foreach (Pawn pawn in TempPawns)
        {
            caravan.RemovePawn(pawn);
            fixedCaravan.AddPawn(pawn);
        }
        TempPawns.Clear();

        GivePawnsOrThings(fixedCaravan, caravan.AllThings.ToList());
        caravan.Destroy();

    }
    public static void ConvertToCaravan(FixedCaravan fixedCaravan)
    {
        TempPawns.Clear();
        TempPawns.AddRange(fixedCaravan.PawnsListForReading);
        fixedCaravan.RemoveAllPawns();
        Caravan caravan = CaravanMaker.MakeCaravan(TempPawns, fixedCaravan.Faction, fixedCaravan.Tile, addToWorldPawnsIfNotAlready: true);
        if (Find.WorldSelector.IsSelected(fixedCaravan))
        {
            Find.WorldSelector.Select(caravan, playSound: false);
        }
        fixedCaravan.Notify_ConvertToCaravan();
        fixedCaravan.Destroy();
        TempPawns.Clear();
    }
    public static void GiveThing(FixedCaravan fixedCaravan, Thing thing)
    {
        if (AllInventoryItems(fixedCaravan).Contains(thing))
        {
            Log.Error(string.Concat("Tried to give the same item twice (", thing, ") to a caravan (", fixedCaravan, ")."));
            return;
        }
        Pawn pawn = CaravanInventoryUtility.FindPawnToMoveInventoryTo(thing, fixedCaravan.PawnsListForReading, null);
        if (pawn == null)
        {
            Log.Error($"Failed to give item {thing} to caravan {fixedCaravan}; item was lost");
            thing.Destroy();
        }
        else if (!pawn.inventory.innerContainer.TryAdd(thing))
        {
            Log.Error($"Failed to give item {thing} to caravan {fixedCaravan}; item was lost");
            thing.Destroy();
        }
    }
    public static void GivePawnsOrThings(FixedCaravan fixedCaravan, List<Thing> things)
    {
        TempAddedItems.Clear();
        TempAddedItems.AddRange(things);
        foreach (Thing thing in TempAddedItems) 
        {
            fixedCaravan.AddPawnOrItem(thing);
        }
        TempAddedItems.Clear();
    }
    public static bool IsExactTypeCaravan(object caravan)
    {
        if (caravan == null)
        {
            return false;
        }
        if (caravan.GetType() == typeof(Caravan))
        {
            return true;
        }
        Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("OA_WarningAbnormalCaravan".Translate(), null, destructive: false, title: "OA_WarningAbnormalCaravanTitle".Translate()));
        return false;
    }
}