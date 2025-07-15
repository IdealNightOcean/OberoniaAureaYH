using RimWorld;

namespace OberoniaAurea;

[DefOf]
public static class OARK_QuestScriptDefOf
{
    public static QuestScriptDef OARK_ProspectingTeam; //路过的勘探队
    public static QuestScriptDef OA_BestowingCeremony; //金鸢尾兰授勋仪式

    public static QuestScriptDef OA_FestivalOrders; //节日订单
    public static QuestScriptDef OA_OpportunitySite_MultiPartyTalks; //多方会谈
    public static QuestScriptDef OA_OpportunitySite_PunishmentExecutor; //惩戒的执行者
    public static QuestScriptDef OA_ResearchSummitSamePeopleJoin; //科研峰会：同道中人
    public static QuestScriptDef OA_ResearcherVisit;  //科研峰会：学者访问
    public static QuestScriptDef OA_UrbanConstruction; //城镇建设需求

    static OARK_QuestScriptDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(OARK_QuestScriptDefOf));
    }
}