using RimWorld;
using Verse;

namespace OberoniaAurea;

public class ThoughtWorker_Precept_Snow : ThoughtWorker_Precept
{
    protected override ThoughtState ShouldHaveThought(Pawn p)
    {
        if (!p.Spawned)
        {
            return ThoughtState.Inactive;
        }
        Map map = p.Map;
        WeatherDef curWeather = map.weatherManager.curWeather;
        if (curWeather == OARK_RimWorldDefOf.SnowGentle || curWeather == OARK_RimWorldDefOf.SnowHard || map.snowGrid.TotalDepth > 1000)
        {
            return ThoughtState.ActiveDefault;
        }
        return ThoughtState.Inactive;
    }
}
