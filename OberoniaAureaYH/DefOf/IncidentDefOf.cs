using RimWorld;

namespace OberoniaAurea;

[DefOf]
public static class OARK_IncidentDefOf
{
    public static IncidentDef OARK_LargeScaleTraderArrival;
    ///<summary>礼物信件</summary>
    public static IncidentDef OARK_SpecialGlobalEvent_GiftLetter;

    ///<summary>生日问候</summary>
    public static IncidentDef OARK_BirthdayWishes;

    [MayRequireOdyssey]
    ///<summary>坠毁的飞船后续</summary>
    public static IncidentDef OARK_CrashedGravship_FollowUp;

    [MayRequireOdyssey]
    ///<summary>科学部友好事件</summary>
    public static IncidentDef OARK_ScienceDepartmentFriendly;
    [MayRequireOdyssey]
    ///<summary>科学部年度互动</summary>
    public static IncidentDef OARK_ScienceDepartmentAnnualInteraction;

    [MayRequireOdyssey]
    ///<summary>回收科研舱</summary>
    public static IncidentDef OARK_ScienceShipRecycle;
    [MayRequireOdyssey]
    ///<summary>冲突地带 - OA支援武装</summary>
    public static IncidentDef OARK_ScienceShip_OASupport;
    [MayRequireOdyssey]
    ///<summary>科研舱后续 - 旅鼠感激</summary>
    public static IncidentDef OARK_ScienceShip_TravelRKReward;
    [MayRequireOdyssey]
    ///<summary>统筹部审查结果</summary>
    public static IncidentDef OARK_EconomyMinistryReview_Outcome;



    static OARK_IncidentDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(OARK_IncidentDefOf));
    }
}
