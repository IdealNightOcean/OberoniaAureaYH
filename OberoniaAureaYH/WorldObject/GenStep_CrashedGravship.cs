using RimWorld;
using RimWorld.SketchGen;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OberoniaAurea;

public class GenStep_CrashedGravship : GenStep
{
    public override int SeedPart => 2031648932;
    public override void Generate(Map map, GenStepParams parms)
    {
        if (!ModsConfig.OdysseyActive)
        {
            return;
        }
        List<Thing> startingItems = GenerateStartingItems();

        DoGravship(map, startingItems);
    }
    private static void DoGravship(Map map, List<Thing> startingItems)
    {
        Sketch sketch = SketchGen.Generate(parms: new SketchResolveParams
        {
            sketch = new Sketch()
        },
                                           root: OARK_ModDefOf.OARK_Sketch_CrashedGravship);

        sketch.Rotate(Rot4.Random);
        HashSet<IntVec3> hashSet = [.. sketch.OccupiedRect.Cells.Select(c => c - sketch.OccupiedCenter)];
        List<CellRect> orGenerateVar = MapGenerator.GetOrGenerateVar<List<CellRect>>("UsedRects");
        map.regionAndRoomUpdater.Enabled = true;

        IntVec3 prePlayerStartSpot = MapGenerator.PlayerStartSpot;
        IntVec3 centerSpot = MapGenerator.PlayerStartSpot;
        if (!MapGenerator.PlayerStartSpotValid)
        {
            GenStep_ReserveGravshipArea.SetStartSpot(map, hashSet, orGenerateVar);
            centerSpot = MapGenerator.PlayerStartSpot;
        }
        MapGenerator.PlayerStartSpot = prePlayerStartSpot;
        GravshipPlacementUtility.ClearAreaForGravship(map, centerSpot, hashSet);

        List<Thing> spawnThings = [];

        sketch.Spawn(map: map,
                     pos: centerSpot,
                     faction: null,
                     posType: Sketch.SpawnPosType.OccupiedCenter,
                     spawnMode: Sketch.SpawnMode.Normal,
                     wipeIfCollides: true,
                     forceTerrainAffordance: true,
                     clearEdificeWhereFloor: true,
                     spawnedThings: spawnThings,
                     dormant: false,
                     buildRoofsInstantly: true);

        IntVec3 offset = centerSpot - sketch.OccupiedCenter;
        CellRect cellRect = sketch.OccupiedRect.MovedBy(offset);
        orGenerateVar.Add(cellRect);

        foreach (Thing startingItem in startingItems)
        {
            int totalStackCount = startingItem.stackCount;
            int num2 = 99;
            while (totalStackCount > 0 && num2-- > 0)
            {
                if (spawnThings.Where(t => t.def == ThingDefOf.Shelf || t.def == ThingDefOf.ShelfSmall).TryRandomElement(out Thing splitItem))
                {
                    IntVec3 randomCell = splitItem.OccupiedRect().RandomCell;
                    Thing thing = startingItem.SplitOff(Math.Min(startingItem.def.stackLimit, totalStackCount));
                    totalStackCount -= thing.stackCount;
                    GenPlace.TryPlaceThing(thing, randomCell, map, ThingPlaceMode.Near);
                }
            }
        }
        foreach (Thing spawnThing in spawnThings)
        {
            if (spawnThing.def == ThingDefOf.Door)
            {
                MapGenerator.rootsToUnfog.AddRange(GenAdj.CellsAdjacentCardinal(spawnThing));
            }
            if (spawnThing.TryGetComp(out CompRefuelable comp))
            {
                comp.Refuel(comp.Props.fuelCapacity * Rand.Range(0.2f, 0.6f));
            }
            if (spawnThing is Building_GravEngine building_GravEngine)
            {
                IntVec3 pos = spawnThing.Position;
                spawnThing.Destroy();
                Thing popper = ThingMaker.MakeThing(ThingDefOf.FirefoamPopper);
                GenPlace.TryPlaceThing(popper, pos, map, ThingPlaceMode.Near);
            }
        }
    }

    private static List<Thing> GenerateStartingItems()
    {
        List<Thing> startingItems = [];

        AddThing(ThingDefOf.Silver, Rand.RangeInclusive(300, 600));
        AddThing(ThingDefOf.MealSurvivalPack, Rand.RangeInclusive(15, 20));
        AddThing(ThingDefOf.Steel, Rand.RangeInclusive(50, 100));
        AddThing(ThingDefOf.GravlitePanel, Rand.RangeInclusive(10, 40));
        AddThing(ThingDefOf.Filth_Fuel, Rand.RangeInclusive(50, 150));
        AddThing(ThingDefOf.ComponentIndustrial, Rand.RangeInclusive(3, 6));
        AddThing(ThingDefOf.ComponentIndustrial, Rand.RangeInclusive(3, 6));

        Book book = (Book)ThingMaker.MakeThing(Rand.Bool ? ThingDefOf.Novel : ThingDefOf.TextBook);
        book.stackCount = 1;
        book.GetComp<CompQuality>()?.SetQuality((QualityCategory)Rand.RangeInclusive(3, 6), ArtGenerationContext.Outsider);
        startingItems.Add(book);

        ThingSetMakerParams makerParams = new()
        {
            countRange = IntRange.One,
            qualityGenerator = QualityGenerator.Super
        };
        ThingSetMaker_UniqueWeapon thingSetMaker_UniqueWeapon = new();
        startingItems.AddRange(thingSetMaker_UniqueWeapon.Generate(makerParams));

        return startingItems;

        void AddThing(ThingDef def, int count = 1)
        {
            Thing thing = ThingMaker.MakeThing(def);
            thing.stackCount = count;
            startingItems.Add(thing);
        }
    }
}
