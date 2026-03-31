using RimWorld;
using Verse;

namespace OberoniaAurea;

[DefOf]
public static class OARK_RimWorldDefOf
{
    ///<summary>合成纤维</summary>
    public static ThingDef Synthread;
    ///<summary>护盾核心</summary>
    public static ThingDef BroadshieldCore;
    ///<summary>虚空电池</summary>
    public static ThingDef VanometricPowerCell;
    [MayRequireBiotech]
    ///<summary>死眠扩容血清</summary>
    public static ThingDef DeathrestCapacitySerum;
    [MayRequireOdyssey]
    ///<summary>感知催化剂</summary>
    public static ThingDef SentienceCatalyst;

    public static SoundDef Emp_Crack;

    public static TraderKindDef Visitor_Outlander_Standard;

    public static WeatherDef SnowHard;
    public static WeatherDef SnowGentle;

    public static QuestScriptDef OpportunitySite_PeaceTalks;
    [MayRequireOdyssey]
    ///<summary>任务-远古军械库</summary>
    public static QuestScriptDef Opportunity_AncientStructureGarrison;
    [MayRequireOdyssey]
    ///<summary>任务-阿尔法敲击兽</summary>
    public static QuestScriptDef OpportunitySite_AlphaThrumbo_Giver;

    public static TraitDef NaturalMood;

    static OARK_RimWorldDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(OARK_RimWorldDefOf));
    }
}