using RimWorld;
using Verse;

namespace OberoniaAurea;

[DefOf]
public static class OARK_RimWorldDefOf
{
    public static ThingDef Synthread; //合成纤维
    public static ThingDef BroadshieldCore; //护盾核心
    public static ThingDef VanometricPowerCell; //虚空电池
    [MayRequireBiotech]
    public static ThingDef DeathrestCapacitySerum; //死眠扩容血清
    [MayRequireOdyssey]
    public static ThingDef SentienceCatalyst; //感知催化剂

    public static TraderKindDef Visitor_Outlander_Standard;

    public static WeatherDef SnowHard;
    public static WeatherDef SnowGentle;

    public static QuestScriptDef OpportunitySite_PeaceTalks;

    public static TraitDef NaturalMood;

    static OARK_RimWorldDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(OARK_RimWorldDefOf));
    }
}