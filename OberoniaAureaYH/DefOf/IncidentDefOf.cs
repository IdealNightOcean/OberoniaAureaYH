using RimWorld;

namespace OberoniaAurea;

[DefOf]
public static class OARK_IncidentDefOf
{
    public static IncidentDef OARK_LargeScaleTraderArrival;
    ///<summary>
    ///礼物信件
    ///</summary>
    public static IncidentDef OARK_SpecialGlobalEvent_GiftLetter;

    ///<summary>
    ///生日问候
    ///</summary>
    public static IncidentDef OARK_BirthdayWishes;

    ///<summary>
    ///坠毁的飞船后续
    ///</summary>
    [MayRequireOdyssey]
    public static IncidentDef OARK_CrashedGravship_FollowUp;

    ///<summary>
    ///科学部友好事件
    ///</summary>
    [MayRequireOdyssey]
    public static IncidentDef OARK_ScienceDepartmentFriendly;

    ///<summary>
    ///科学部年度互动
    ///</summary>
    [MayRequireOdyssey]
    public static IncidentDef OARK_ScienceDepartmentAnnualInteraction;

    ///<summary>
    ///回收科研舱
    ///</summary>
    [MayRequireOdyssey]
    public static IncidentDef OARK_ScienceShipRecycle;

    ///<summary>
    ///冲突地带 - OA支援武装
    ///</summary>
    [MayRequireOdyssey]
    public static IncidentDef OARK_ScienceShip_OASupport;

    ///<summary>
    ///科研舱后续 - 旅鼠感激
    ///</summary>
    [MayRequireOdyssey]
    public static IncidentDef OARK_ScienceShip_TravelRKReward;

    ///<summary>
    ///统筹部审查结果
    ///</summary>
    [MayRequireOdyssey]
    public static IncidentDef OARK_EconomyMinistryReview_Outcome;


    static OARK_IncidentDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(OARK_IncidentDefOf));
    }
}
