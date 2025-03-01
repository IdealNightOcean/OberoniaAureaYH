﻿using RimWorld;
using Verse;

namespace OberoniaAurea;

[DefOf]
public static class OberoniaAureaYHDefOf
{
    public static FactionDef OA_RK_Faction; //金鼠鼠派系

    public static GameConditionDef OA_MilitaryDeployment; //军事部署

    public static IncidentDef OA_RK_LargeScaleTraderArrival;

    public static JobDef OA_RK_RepairCircuitRegulator; //整修电路调控

    public static LetterDef OA_RK_AcceptJoinerViewInfo;

    //public static PawnsArrivalModeDef OA_RK_CenterDrop;
    public static PreceptDef OA_RK_MentalBreakProbability_Atonement;
    public static PreceptDef OA_RK_MentalBreakProbability_Low;

    public static QuestScriptDef OA_OpportunitySite_MultiPartyTalks; //多方会谈
    public static QuestScriptDef OA_OpportunitySite_PunishmentExecutor; //惩戒的执行者
    public static QuestScriptDef OA_ResearchSummitSamePeopleJoin; //科研峰会：同道中人
    public static QuestScriptDef OA_ResearcherVisit;  //科研峰会：学者访问

    //public static RaidStrategyDef OA_RK_DropAttack;

    public static StorytellerDef Begonia_Oberonia_Aurea;

    static OberoniaAureaYHDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(OberoniaAureaYHDefOf));
    }
}

[DefOf]
public static class OA_ThingDefOf
{
    public static ThingDef OA_RK_CircuitRegulator;
    public static ThingDef OA_RK_Cloth_Processing_B;
    public static ThingDef Oberonia_Aurea_Tea; //花茶
    public static ThingDef Oberonia_Aurea_Chanwu_AB; //花糕
    public static ThingDef Oberonia_Aurea_Chanwu_AC; //流心花糕
    public static ThingDef Oberonia_Aurea_Chanwu_F; //浓缩液
    public static ThingDef Oberonia_Aurea_Chanwu_G; //提取液
    static OA_ThingDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(OA_ThingDefOf));
    }
}

[DefOf]
public static class OA_PawnInfoDefOf
{
    public static HediffDef OA_RK_Hediff_ColdAdaptation;
    public static HediffDef OA_RK_Hediff_HotAdaptation;
    public static HediffDef OA_RK_SeriousInjuryI;
    public static HediffDef OA_RK_SeriousInjuryII;

    public static ThoughtDef OA_RK_Atonement;
    public static ThoughtDef OA_RK_ResponsibilityConstraints;

    static OA_PawnInfoDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(OA_PawnInfoDefOf));
    }
}

[DefOf]
public static class OA_PawnGenerateDefOf
{
    public static PawnKindDef OA_RK_Traveller;

    public static PawnGroupKindDef OA_Rk_LargeScaleTrader;

    public static StandalonePawnGroupMakerDef OA_ResearchSummit_TravellerMaker;

    public static TraderKindDef OA_RK_Caravan_TributeCollector; //物质征募队
    public static TraderKindDef OA_ResearchSummit_FairTrader; //科研峰会：集市商人
    public static TraderKindDef OA_ResearchSummit_TravellerTrader; //科研峰会：参会旅行者

    static OA_PawnGenerateDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(OA_PawnGenerateDefOf));
    }
}

[DefOf]
public static class OA_WorldObjectDefOf
{
    public static SitePartDef OA_RK_ResearcherCamp; //科研峰会：落脚点

    public static WorldObjectDef OA_RK_ResearchSummit; //科研峰会
    public static WorldObjectDef OA_ResearchSummit_AcademicDispute; //科研峰会：约架
    public static WorldObjectDef OA_ResearchSummit_Fair;
    public static WorldObjectDef OA_ResearchSummit_MysteriousTrader;
    public static WorldObjectDef OA_ResearchSummit_AssistWork;
    public static WorldObjectDef OA_FixedCaravan_ResearchSummitAssistWork;
    public static WorldObjectDef OA_FixedCaravan_DiplomaticSummit;
    static OA_WorldObjectDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(OA_WorldObjectDefOf));
    }
}

[DefOf]
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