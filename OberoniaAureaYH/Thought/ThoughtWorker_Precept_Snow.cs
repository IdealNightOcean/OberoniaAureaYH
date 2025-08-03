using OberoniaAurea_Frame;
using RimWorld;
using Verse;

namespace OberoniaAurea;

public class ThoughtWorker_Precept_Snow : ThoughtWorker_Precept
{
    private static SimpleMapCahce<bool> mapCahce = new(cacheInterval: 15000, onlyPlayerHome: false, checker: Checker);
    protected override ThoughtState ShouldHaveThought(Pawn p)
    {
        if (!p.Spawned)
        {
            return ThoughtState.Inactive;
        }

        return mapCahce.GetCachedResult(p.Map) ? ThoughtState.ActiveDefault : ThoughtState.Inactive;
    }

    private static bool Checker(Map map)
    {
        WeatherDef curWeather = map.weatherManager.curWeather;
        if (curWeather == OARK_RimWorldDefOf.SnowGentle || curWeather == OARK_RimWorldDefOf.SnowHard || map.snowGrid.TotalDepth > 1000f)
        {
            return true;
        }
        return false;
    }
    public static void ClearStaticCache() => mapCahce.Reset();
}
