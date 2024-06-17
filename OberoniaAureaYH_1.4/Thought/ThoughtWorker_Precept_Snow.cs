using RimWorld;
using Verse;

namespace OberoniaAurea;

public class ThoughtWorker_Precept_Snow : ThoughtWorker_Precept
{
    protected override ThoughtState ShouldHaveThought(Pawn p)
    {
        if (!ModsConfig.IdeologyActive || !p.Spawned)
        {
            return ThoughtState.Inactive;
        }
        Map map = p.Map;
        if (map == null)
        {
            return ThoughtState.Inactive;
        }
        if (map.weatherManager.curWeather == OA_RimWorldDefOf.SnowGentle || map.weatherManager.curWeather == OA_RimWorldDefOf.SnowHard || map.snowGrid.TotalDepth > 1000)
        {
            return ThoughtState.ActiveDefault;
        }
        return ThoughtState.Inactive;
    }
}
