using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace OberoniaAurea;

public static class FixedCaravanUtility
{
    private static readonly List<Thing> TempInventoryItems = [];
    private static readonly List<Pawn> TempPawns = [];
    public static List<Thing> OriginInventoryItems(FixedCaravan fixedCaravan)
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
        ConvertToFixCaravan(caravan, fixedCaravan);
        return fixedCaravan;
    }
    public static void ConvertToFixCaravan(Caravan caravan, FixedCaravan fixedCaravan)
    {
        TempPawns.Clear();
        TempPawns.AddRange(caravan.PawnsListForReading);
        foreach (Pawn pawn in TempPawns)
        {
            caravan.RemovePawn(pawn);
            fixedCaravan.AddPawn(pawn);
        }
        fixedCaravan.NewItemsForReading.AddRange(caravan.AllThings);
        caravan.Destroy();
    }
    public static void ConvertToCaravan(FixedCaravan fixedCaravan)
    {
        TempPawns.Clear();
        TempPawns.AddRange(fixedCaravan.PawnsListForReading);
        fixedCaravan.RemoveAllPawns();
        Caravan caravan = CaravanMaker.MakeCaravan(TempPawns, fixedCaravan.Faction, fixedCaravan.Tile, addToWorldPawnsIfNotAlready: true);
        List<Thing> newItems = fixedCaravan.NewItemsForReading;
        if (newItems != null)
        {
            foreach (Thing item in newItems.Except(caravan.AllThings))
            {
                caravan.AddPawnOrItem(item, addCarriedPawnToWorldPawnsIfAny: true);
            }
        }
        if (Find.WorldSelector.IsSelected(fixedCaravan))
        {
            Find.WorldSelector.Select(caravan, playSound: false);
        }
        fixedCaravan.Notify_ConvertToCaravan();
        fixedCaravan.Destroy();
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
        Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("OA_WaringAbnormalCaravan".Translate(), null, destructive: false, title: "OA_WaringAbnormalCaravanTitle".Translate()));
        return false;
    }
}