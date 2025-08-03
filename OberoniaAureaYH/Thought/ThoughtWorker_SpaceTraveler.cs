using OberoniaAurea_Frame;
using RimWorld;
using Verse;

namespace OberoniaAurea;

public class ThoughtWorker_SpaceTraveler : ThoughtWorker
{
    private static SimpleMapCahce<bool> mapCahce = new(cacheInterval: 5000, onlyPlayerHome: false, checker: Checker);

    protected override ThoughtState CurrentStateInternal(Pawn p)
    {
        if (!ModsConfig.OdysseyActive || !p.Spawned)
        {
            return ThoughtState.Inactive;
        }

        return mapCahce.GetCachedResult(p.MapHeld) ? ThoughtState.ActiveDefault : ThoughtState.Inactive;
    }

    private static bool Checker(Map map) => !map.Tile.LayerDef.isSpace;

    public static void ClearStaticCache() => mapCahce.Reset();
}
