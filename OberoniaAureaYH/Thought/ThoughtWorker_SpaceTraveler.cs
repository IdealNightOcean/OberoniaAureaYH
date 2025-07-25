using OberoniaAurea_Frame;
using RimWorld;
using Verse;

namespace OberoniaAurea;


public class ThoughtWorker_SpaceTraveler : ThoughtWorker_Precept
{
    private static SimpleMapCahce<bool> mapCahce = new(cacheInterval: 10000, onlyPlayerHome: false, checker: Checker);

    protected override ThoughtState ShouldHaveThought(Pawn p)
    {
        if (!ModsConfig.OdysseyActive || !p.SpawnedOrAnyParentSpawned)
        {
            return ThoughtState.Inactive;
        }
        if (mapCahce.GetCachedResult(p.MapHeld))
        {
            return ThoughtState.ActiveDefault;
        }
        return ThoughtState.Inactive;
    }

    private static bool Checker(Map map)
    {
        return !map.Tile.LayerDef.isSpace;
    }

    public static void ClearStaticCache()
    {
        mapCahce.Reset();
    }
}
