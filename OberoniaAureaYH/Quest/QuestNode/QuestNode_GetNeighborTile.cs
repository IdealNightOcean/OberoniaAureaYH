using RimWorld.Planet;
using RimWorld.QuestGen;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OberoniaAurea;

public class QuestNode_GetNeighborTile : QuestNode
{
    [NoTranslate]
    public SlateRef<string> storeAs;

    public SlateRef<int> centerTile = Tile.Invalid;
    public SlateRef<bool> mustEmpty = true;

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
    protected bool TryFindTile(Slate slate, out int tile)
    {
        if (centerTile.GetValue(slate) == Tile.Invalid)
        {
            tile = Tile.Invalid;
            return false;
        }
        List<int> neighborTiles = [];
        Find.WorldGrid.GetTileNeighbors(centerTile.GetValue(slate), neighborTiles);
        if (neighborTiles.Any())
        {
            if (mustEmpty.GetValue(slate))
            {
                WorldObjectsHolder worldObjects = Find.WorldObjects;
                foreach (int item in neighborTiles)
                {
                    if (!worldObjects.AnyWorldObjectAt(item))
                    {
                        tile = item;
                        return true;
                    }
                }
                tile = Tile.Invalid;
                return false;
            }
            else
            {
                tile = neighborTiles.RandomElement();
                return true;
            }
        }
        else
        {
            tile = Tile.Invalid;
            return false;
        }
    }
}
