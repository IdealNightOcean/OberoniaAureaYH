using RimWorld;
using Verse;

namespace OberoniaAurea;

[DefOf]
public static class OberoniaAureaYHDefOf
{
    public static FactionDef OA_RK_Faction; //金鼠鼠派系

    public static GameConditionDef OA_MilitaryDeployment; //军事部署

    public static GeneDef OA_RK_Gene_ColdAdaptation;
    public static GeneDef OA_RK_Gene_HotAdaptation;

    public static HediffDef OA_RK_SeriousInjuryI;
    public static HediffDef OA_RK_SeriousInjuryII;

    public static IncidentDef OA_RK_LargeScaleTraderArrival;

    public static JobDef OA_RK_RepairCircuitRegulator; //整修电路调控

    public static LetterDef OA_RK_AcceptJoinerViewInfo;

    //public static PawnsArrivalModeDef OA_RK_CenterDrop;
    public static PawnKindDef OA_RK_Traveller;
    public static PawnGroupKindDef OA_Rk_LargeScaleTrader;

    public static PreceptDef OA_RK_MentalBreakProbability_Atonement;
    public static PreceptDef OA_RK_MentalBreakProbability_Low;

    public static QuestScriptDef OA_OpportunitySite_MultiPartyTalks; //多方会谈
    public static QuestScriptDef OA_OpportunitySite_PunishmentExecutor; //惩戒的执行者
    public static QuestScriptDef OA_ResearchSummitSamePeopleJoin; //研究峰会 - 同道中人
    public static QuestScriptDef OA_ResearcherVisit;  //研究峰会 - 无势力学者的访问

    //public static RaidStrategyDef OA_RK_DropAttack;

    public static SitePartDef OA_RK_ResearcherCamp;
    public static StorytellerDef Begonia_Oberonia_Aurea;

    public static ThingDef OA_RK_CircuitRegulator;
    public static ThingDef OA_RK_Cloth_Processing_B;
    public static ThingDef Oberonia_Aurea_Tea; //花茶
    public static ThingDef Oberonia_Aurea_Chanwu_AB; //花糕
    public static ThingDef Oberonia_Aurea_Chanwu_AC; //流心花糕
    public static ThingDef Oberonia_Aurea_Chanwu_F; //浓缩液
    public static ThingDef Oberonia_Aurea_Chanwu_G; //提取液

    public static ThoughtDef OA_RK_Atonement;
    public static ThoughtDef OA_RK_ResponsibilityConstraints;

    public static TraderKindDef OA_RK_Caravan_TributeCollector; //物质征募队

    public static WorldObjectDef OA_RK_AcademicDispute; //学术约架

    static OberoniaAureaYHDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(OberoniaAureaYHDefOf));
    }
}

public static class OA_HistoryEventDefOf
{
    public static HistoryEventDef OA_ResearchSummit;
    public static HistoryEventDef OA_AttackResearcherCamp;
    public static HistoryEventDef OA_DiplomaticSummit_LeaveHalfway;
    public static HistoryEventDef OA_DiplomaticSummit_Disaster;
    public static HistoryEventDef OA_DiplomaticSummit_Flounder;
    public static HistoryEventDef OA_DiplomaticSummit_Triumph;

    static OA_HistoryEventDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(OA_HistoryEventDefOf));
    }
}


[DefOf]
public static class OA_RimWorldDefOf
{
    public static WeatherDef SnowHard;
    public static WeatherDef SnowGentle;
    public static QuestScriptDef OpportunitySite_PeaceTalks;
    public static TraitDef NaturalMood;

    static OA_RimWorldDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(OA_RimWorldDefOf));
    }
}