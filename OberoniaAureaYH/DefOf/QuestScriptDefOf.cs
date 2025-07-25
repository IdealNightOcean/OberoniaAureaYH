using RimWorld;

namespace OberoniaAurea;

[DefOf]
public static class OARK_QuestScriptDefOf
{
    public static QuestScriptDef OA_BestowingCeremony; //金鸢尾兰授勋仪式

    public static QuestScriptDef OA_FestivalOrders; //节日订单
    public static QuestScriptDef OA_OpportunitySite_MultiPartyTalks; //多方会谈
    public static QuestScriptDef OA_OpportunitySite_PunishmentExecutor; //惩戒的执行者
    public static QuestScriptDef OA_ResearchSummitSamePeopleJoin; //科研峰会：同道中人
    public static QuestScriptDef OA_ResearcherVisit;  //科研峰会：学者访问
    public static QuestScriptDef OA_UrbanConstruction; //城镇建设需求

    [MayRequireOdyssey]
    public static QuestScriptDef OARK_InitGravQuest; //科学部初见
    [MayRequireOdyssey]
    public static QuestScriptDef OARK_ScienceDepartment_SailorAssistance;
    [MayRequireOdyssey]
    public static QuestScriptDef OARK_CrashedGravship; //坠毁的飞船
    [MayRequireOdyssey]
    public static QuestScriptDef OARK_ScienceShipRecycle; //回收科学船
    [MayRequireOdyssey]
    public static QuestScriptDef OARK_GravResearchAssistance; //回收科学船
    [MayRequireOdyssey]
    public static QuestScriptDef OARK_GravResearchExchange; //友好事件：研究交流



    static OARK_QuestScriptDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(OARK_QuestScriptDefOf));
    }
}