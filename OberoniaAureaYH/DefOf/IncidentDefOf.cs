using RimWorld;

namespace OberoniaAurea;

[DefOf]
public static class OARK_IncidentDefOf
{
    public static IncidentDef OARK_LargeScaleTraderArrival;
    public static IncidentDef OARK_NewYearEvent; //新年慰问
    [MayRequireOdyssey]
    public static IncidentDef OARK_CrashedGravship_FollowUp; //坠毁的飞船后续

    [MayRequireOdyssey]
    public static IncidentDef OARK_ScienceDepartmentFriendly; //科学部友好事件
    [MayRequireOdyssey]
    public static IncidentDef OARK_ScienceDepartmentAnnualInteraction; //科学部年度互动

    [MayRequireOdyssey]
    public static IncidentDef OARK_ScienceShipRecycle; //回收科研舱
    [MayRequireOdyssey]
    public static IncidentDef OARK_ScienceShip_OASupport; //冲突地带 - OA支援武装
    [MayRequireOdyssey]
    public static IncidentDef OARK_ScienceShip_TravelRKReward; //科研舱后续 - 旅鼠感激

    static OARK_IncidentDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(OARK_IncidentDefOf));
    }
}
