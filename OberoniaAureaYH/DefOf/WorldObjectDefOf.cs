using RimWorld;

namespace OberoniaAurea;

[DefOf]
public static class OARK_WorldObjectDefOf
{
    public static SitePartDef OA_RK_ResearcherCamp; //科研峰会：落脚点

    public static WorldObjectDef OA_RK_ResearchSummit; //科研峰会
    public static WorldObjectDef OA_ResearchSummit_AcademicDispute; //科研峰会：约架
    public static WorldObjectDef OA_ResearchSummitEccentricScholar;
    public static WorldObjectDef OA_ResearchSummit_Fair;
    public static WorldObjectDef OA_ResearchSummit_MysteriousTrader;
    public static WorldObjectDef OA_ResearchSummit_AssistWork;

    public static WorldObjectDef OA_FixedCaravan_DiplomaticSummit;

    [MayRequireOdyssey]
    public static WorldObjectDef OARK_CrashedScienceShip;
    [MayRequireOdyssey]
    public static WorldObjectDef OARK_FixedCaravan_ScienceShipLaunch;

    static OARK_WorldObjectDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(OARK_WorldObjectDefOf));
    }
}
