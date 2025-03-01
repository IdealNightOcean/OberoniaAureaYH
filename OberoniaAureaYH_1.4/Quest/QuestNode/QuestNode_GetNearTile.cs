﻿using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace OberoniaAurea;

public class QuestNode_GetNearTile : QuestNode
{
    [NoTranslate]
    public SlateRef<string> storeAs;

    public SlateRef<WorldObject> worldObject;
    public SlateRef<int> centerTile = Tile.Invalid;
    public SlateRef<bool> mustEmpty = true;
    public SlateRef<bool> neighborFirst;
    public SlateRef<int> minDist = 0;
    public SlateRef<int> maxDist = 999;

    protected override bool TestRunInt(Slate slate)
    {
        if (TryFindTile(slate, out var tile))
        {
            slate.Set(storeAs.GetValue(slate), tile);
        }
        return true;
    }
    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        if (!slate.TryGet<int>(storeAs.GetValue(slate), out var _) && TryFindTile(QuestGen.slate, out var tile))
        {
            QuestGen.slate.Set(storeAs.GetValue(slate), tile);
        }
    }
    protected bool ResloveCenterTile(Slate slate, out int tile)
    {
        tile = Tile.Invalid;
        if (centerTile.GetValue(slate) != Tile.Invalid)
        {
            tile = centerTile.GetValue(slate);
            return true;
        }
        if (worldObject.GetValue(slate) != null)
        {
            tile = worldObject.GetValue(slate).Tile;
            return tile != Tile.Invalid;
        }
        return false;
    }

    protected bool TryFindTile(Slate slate, out int tile)
    {
        tile = Tile.Invalid;
        if (!ResloveCenterTile(slate, out int rootTile))
        {
            return false;
        }
        if (neighborFirst.GetValue(slate))
        {
            if (OberoniaAureaYHUtility.GetAvailableNeighborTile(rootTile, out tile, mustEmpty.GetValue(slate)))
            {
                return true;
            }
        }
        tile = Tile.Invalid;
        WorldObjectsHolder worldObjects = Find.WorldObjects;
        int minDist = this.minDist.GetValue(slate);
        int maxDist = this.maxDist.GetValue(slate);
        bool findFlag;
        if (mustEmpty.GetValue(slate))
        {
            findFlag = TileFinder.TryFindPassableTileWithTraversalDistance(rootTile, minDist, maxDist, out tile, (int t) => !worldObjects.AnyWorldObjectAt(t), tileFinderMode: TileFinderMode.Near);
        }
        else
        {
            findFlag = TileFinder.TryFindPassableTileWithTraversalDistance(rootTile, minDist, maxDist, out tile, tileFinderMode: TileFinderMode.Near);
        }
        if (findFlag)
        {
            return true;
        }
        return TileFinder.TryFindNewSiteTile(out tile);
    }
}
