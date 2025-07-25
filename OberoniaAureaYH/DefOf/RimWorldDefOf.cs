using RimWorld;
using Verse;

namespace OberoniaAurea;

[DefOf]
public static class OARK_RimWorldDefOf
{
    public static ThingDef Synthread;
    public static ThingDef VanometricPowerCell;

    public static WeatherDef SnowHard;
    public static WeatherDef SnowGentle;

    public static QuestScriptDef OpportunitySite_PeaceTalks;

    public static TraitDef NaturalMood;

    static OARK_RimWorldDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(OARK_RimWorldDefOf));
    }
}