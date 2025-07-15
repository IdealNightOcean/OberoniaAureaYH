using RimWorld;
using Verse;

namespace OberoniaAurea;

[DefOf]
public static class OARK_RimWorldDefOf
{
    public static WeatherDef SnowHard;
    public static WeatherDef SnowGentle;

    public static QuestScriptDef OpportunitySite_PeaceTalks;

    public static TraitDef NaturalMood;

    public static TraderKindDef Visitor_Outlander_Standard;

    static OARK_RimWorldDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(OARK_RimWorldDefOf));
    }
}