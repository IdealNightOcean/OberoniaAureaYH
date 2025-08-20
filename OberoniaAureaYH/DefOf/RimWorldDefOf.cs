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

    public static SoundDef Emp_Crack;

    public static TraderKindDef Visitor_Outlander_Standard;

    public static WeatherDef SnowHard;
    public static WeatherDef SnowGentle;

    public static QuestScriptDef OpportunitySite_PeaceTalks;
    [MayRequireOdyssey]
    public static QuestScriptDef Opportunity_AncientStructureGarrison; //任务-远古军械库
    [MayRequireOdyssey]
    public static QuestScriptDef OpportunitySite_AlphaThrumbo_Giver; //任务-阿尔法敲击兽

    public static TraitDef NaturalMood;

    static OARK_RimWorldDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(OARK_RimWorldDefOf));
    }
}