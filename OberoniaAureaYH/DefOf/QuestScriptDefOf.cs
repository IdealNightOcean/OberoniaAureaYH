using RimWorld;

namespace OberoniaAurea;

[DefOf]
public static class OARK_QuestScriptDefOf
{
    ///<summary>
    ///路过的勘探队
    ///</summary>
    public static QuestScriptDef OARK_ProspectingTeam;

    ///<summary>
    ///金鸢尾兰授勋仪式
    ///</summary>
    public static QuestScriptDef OA_BestowingCeremony;

    ///<summary>
    ///生日拜访
    ///</summary>
    public static QuestScriptDef OARK_SDBirthdayVisit;


    ///<summary>
    ///节日订单
    ///</summary>
    public static QuestScriptDef OA_FestivalOrders;

    ///<summary>
    ///多方会谈
    ///</summary>
    public static QuestScriptDef OA_OpportunitySite_MultiPartyTalks;

    ///<summary>
    ///惩戒的执行者
    ///</summary>
    public static QuestScriptDef OA_OpportunitySite_PunishmentExecutor;

    ///<summary>
    ///科研峰会：同道中人
    ///</summary>
    public static QuestScriptDef OA_ResearchSummitSamePeopleJoin;

    ///<summary>
    ///科研峰会：学者访问
    ///</summary>
    public static QuestScriptDef OA_ResearcherVisit;

    ///<summary>
    ///城镇建设需求
    ///</summary>
    public static QuestScriptDef OA_UrbanConstruction;

    ///<summary>
    ///科学部初见
    ///</summary>
    [MayRequireOdyssey]
    public static QuestScriptDef OARK_InitGravQuest;

    [MayRequireOdyssey]
    public static QuestScriptDef OARK_ScienceDepartment_SailorsAssistance;

    ///<summary>
    ///坠毁的飞船
    ///</summary>
    [MayRequireOdyssey]
    public static QuestScriptDef OARK_CrashedGravship;

    ///<summary>
    ///回收科学船
    ///</summary>
    [MayRequireOdyssey]
    public static QuestScriptDef OARK_ScienceShipRecycle;

    ///<summary>
    ///回收科学船
    ///</summary>
    [MayRequireOdyssey]
    public static QuestScriptDef OARK_GravResearchAssistance;

    ///<summary>
    ///友好事件：研究交流
    ///</summary>
    [MayRequireOdyssey]
    public static QuestScriptDef OARK_GravResearchExchange;

    ///<summary>
    ///统筹部的审查
    ///</summary>
    [MayRequireOdyssey]
    public static QuestScriptDef OARK_EconomyMinistryReview;


    static OARK_QuestScriptDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(OARK_QuestScriptDefOf));
    }
}