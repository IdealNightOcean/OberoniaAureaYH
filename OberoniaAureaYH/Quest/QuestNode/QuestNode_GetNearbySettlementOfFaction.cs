using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System.Linq;
using Verse;

namespace OberoniaAurea;


public class QuestNode_GetNearbySettlementOfFaction : QuestNode
{
    [NoTranslate]
    public SlateRef<string> storeAs;

    public SlateRef<bool> ignoreDistIfNecessary;

    public SlateRef<float> maxTileDistance;

    public SlateRef<Faction> faction;

    private Settlement RandomNearbySettlement(int originTile, Slate slate)
    {
        Faction faction = this.faction.GetValue(slate);
        if (faction == null)
        {
            return null;
        }
        Settlement tempSettlement = Find.WorldObjects.SettlementBases.Where(delegate (Settlement settlement)
        {
            if (!settlement.Visitable || settlement.Faction != faction)
            {
                return false;
            }
            return Find.WorldGrid.ApproxDistanceInTiles(originTile, settlement.Tile) < maxTileDistance.GetValue(slate) && Find.WorldReachability.CanReach(originTile, settlement.Tile);
        }).RandomElementWithFallback();
        if (ignoreDistIfNecessary.GetValue(slate) && tempSettlement == null)
        {
            tempSettlement = Find.WorldObjects.SettlementBases.Where(delegate (Settlement settlement)
            {
                if (!settlement.Visitable || settlement.Faction != faction)
                {
                    return false;
                }
                return true;
            }).RandomElementWithFallback();
        }
        return tempSettlement;
    }

    protected override void RunInt()
    {
        Slate slate = QuestGen.slate;
        SetVars(slate);
    }

    protected override bool TestRunInt(Slate slate)
    {
        SetVars(slate);
        return true;
    }
    protected void SetVars(Slate slate)
    {
        Map map = slate.Get<Map>("map");
        Settlement settlement = RandomNearbySettlement(map.Tile, slate);
        if (map != null && settlement != null)
        {
            slate.Set(storeAs.GetValue(slate), settlement);
        }
    }
}
