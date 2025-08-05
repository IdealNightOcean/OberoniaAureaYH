using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace OberoniaAurea;

public class QuestPart_ProspectingTeamReward : QuestPart
{
    public string insSignal;
    public string inSignalRemovePawn;
    public Pawn leader;
    public List<Pawn> pawns;
    public MapParent mapParent;
    public Faction faction;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref insSignal, "insSignal");
        Scribe_Values.Look(ref inSignalRemovePawn, "inSignalRemovePawn");
        Scribe_References.Look(ref leader, "leader");
        Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
        Scribe_References.Look(ref mapParent, "mapParent");
        Scribe_References.Look(ref faction, "faction");

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            RemoveInvalidPawn();
        }
    }

    public override void Notify_QuestSignalReceived(Signal signal)
    {
        base.Notify_QuestSignalReceived(signal);
        if (signal.tag == insSignal)
        {
            if (mapParent?.HasMap ?? false)
            {
                RemoveInvalidPawn();
                GiveSpecialReward(mapParent.Map, faction, leader ?? pawns?.RandomElementWithFallback(null), quest);
            }
        }
        else if (signal.tag == inSignalRemovePawn)
        {
            if (signal.args.TryGetArg("SUBJECT", out Pawn arg))
            {
                pawns?.Remove(arg);
                if (arg == leader)
                {
                    leader = null;
                }
            }
        }
    }

    public override void Cleanup()
    {
        base.Cleanup();
        insSignal = null;
        inSignalRemovePawn = null;
        leader = null;
        pawns = null;
        mapParent = null;
        faction = null;
    }

    private void RemoveInvalidPawn()
    {
        if (leader is not null && leader.Dead)
        {
            leader = null;
        }
        pawns?.RemoveAll(p => p is null || p.Dead);
    }

    private static void GiveSpecialReward(Map map, Faction faction, Pawn leader, Quest quest = null)
    {
        IntVec3 centerCell = IntVec3.Invalid;
        if (leader is not null && leader.Spawned && CanScatterAt(leader.Position, map))
        {
            centerCell = leader.Position;
        }
        else
        {
            List<Pawn> potentialPawns = map.mapPawns.FreeColonistsAndPrisonersSpawned;
            foreach (Pawn p in potentialPawns)
            {
                if (CanScatterAt(p.Position, map))
                {
                    centerCell = p.Position;
                    break;
                }
            }
        }
        if (!centerCell.IsValid && !CellFinderLoose.TryFindRandomNotEdgeCellWith(10, c => CanScatterAt(c, map), map, out centerCell))
        {
            Log.Error("Could not find a center cell for deep scanning lump generation!");
        }
        ThingDef thingDef = ThingDefOf.Uranium;
        int numCells = Mathf.CeilToInt(thingDef.deepLumpSizeRange.RandomInRange);
        Thing deepDrill = ThingMaker.MakeThing(ThingDefOf.DeepDrill);
        if (deepDrill.def.CanHaveFaction)
        {
            deepDrill.SetFaction(Faction.OfPlayer);
        }
        GenPlace.TryPlaceThing(deepDrill, centerCell, map, ThingPlaceMode.Near);
        int validCount = 0;
        foreach (IntVec3 cell in GridShapeMaker.IrregularLump(centerCell, map, numCells))
        {
            if (CanScatterAt(cell, map) && !cell.InNoBuildEdgeArea(map))
            {
                map.deepResourceGrid.SetAt(cell, thingDef, thingDef.deepCountPerCell);
                if (++validCount >= 3)
                {
                    break;
                }
            }
        }

        TaggedString label;
        TaggedString text;
        if (Faction.OfPlayer.def.defName == "OA_RK_PlayerFaction" || Faction.OfPlayer.def.defName == "OA_RK_PlayerFaction_B")
        {
            label = "OARK_LetterLabel_ProspectingTeamReward_OA".Translate();
            text = "OARK_Letter_ProspectingTeamReward_OA".Translate(leader);

            Thing flowerCakes = ThingMaker.MakeThing(OARK_ThingDefOf.Oberonia_Aurea_Chanwu_AB);
            flowerCakes.stackCount = 10;
            GenPlace.TryPlaceThing(flowerCakes, centerCell, map, ThingPlaceMode.Near);
            Thing Silver = ThingMaker.MakeThing(ThingDefOf.Silver);
            Silver.stackCount = 200;
            GenPlace.TryPlaceThing(Silver, centerCell, map, ThingPlaceMode.Near);

            faction.TryAffectGoodwillWith(Faction.OfPlayer, 100, reason: OARK_HistoryEventDefOf.OARK_ProspectingTeam);
        }
        else
        {
            label = "OARK_LetterLabel_ProspectingTeamReward".Translate();
            text = "OARK_Letter_ProspectingTeamReward".Translate(leader);
        }

        Find.LetterStack.ReceiveLetter(label: label,
                                       text: text,
                                       textLetterDef: LetterDefOf.PositiveEvent,
                                       new LookTargets(deepDrill),
                                       relatedFaction: faction,
                                       quest: quest);
    }

    private static bool CanScatterAt(IntVec3 pos, Map map)
    {
        int index = CellIndicesUtility.CellToIndex(pos, map.Size.x);
        TerrainDef terrainDef = map.terrainGrid.BaseTerrainAt(pos);
        if ((terrainDef is not null && terrainDef.IsWater && terrainDef.passability == Traversability.Impassable)
            || !pos.GetAffordances(map).Contains(ThingDefOf.DeepDrill.terrainAffordanceNeeded))
        {
            return false;
        }
        return !map.deepResourceGrid.GetCellBool(index);
    }
}