using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OberoniaAurea;


public class QuestNode_GetNearbySettlementOfFaction : QuestNode
{
    [NoTranslate]
    public SlateRef<string> storeAs;

    public SlateRef<bool> allowActiveTradeRequest = true;

    public SlateRef<float> maxTileDistance;

    public SlateRef<Faction> faction;

    private Settlement RandomNearbyTradeableSettlement(int originTile, Slate slate)
    {
        Faction faction = this.faction.GetValue(slate);
        if (faction == null)
        {
            return null;
        }
        return Find.WorldObjects.SettlementBases.Where(delegate (Settlement settlement)
        {
            if (!settlement.Visitable)
            {
                return false;
            }
            if (settlement.Faction != faction)
            {
                return false;
            }
            if (!allowActiveTradeRequest.GetValue(slate))
            {
                if (settlement.GetComponent<TradeRequestComp>() != null && settlement.GetComponent<TradeRequestComp>().ActiveRequest)
                {
                    return false;
                }
                List<Quest> questsListForReading = Find.QuestManager.QuestsListForReading;
                for (int i = 0; i < questsListForReading.Count; i++)
                {
                    if (!questsListForReading[i].Historical)
                    {
                        List<QuestPart> partsListForReading = questsListForReading[i].PartsListForReading;
                        for (int j = 0; j < partsListForReading.Count; j++)
                        {
                            if (partsListForReading[j] is QuestPart_InitiateTradeRequest questPart_InitiateTradeRequest && questPart_InitiateTradeRequest.settlement == settlement)
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return Find.WorldGrid.ApproxDistanceInTiles(originTile, settlement.Tile) < maxTileDistance.GetValue(slate) && Find.WorldReachability.CanReach(originTile, settlement.Tile);
        }).RandomElementWithFallback();
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
        Settlement settlement = RandomNearbyTradeableSettlement(map.Tile, slate);
        if (map != null && settlement != null)
        {
            slate.Set(storeAs.GetValue(slate), settlement);
        }
    }
}
