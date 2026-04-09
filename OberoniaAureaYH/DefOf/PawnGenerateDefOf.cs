using OberoniaAurea_Frame;
using RimWorld;
using Verse;

namespace OberoniaAurea;

[DefOf]
public static class OARK_PawnGenerateDefOf
{
    ///<summary>勘探队员</summary>
    public static PawnKindDef OA_RK_Court_Member_Exploration;
    ///<summary>金鸢尾兰侍从///</summary>
    public static PawnKindDef OA_RK_Court_Member;

    public static PawnKindDef OA_RK_Traveller;
    ///<summary>统筹审查官</summary>
    public static PawnKindDef OA_RK_Elite_Court_Member_B;
    ///<summary>金鼠鼠授勋官</summary>
    public static PawnKindDef OA_RK_Noble_C;
    ///<summary>金鼠鼠授勋官护卫</summary>
    public static PawnKindDef OA_RK_Guard_Member;
    ///<summary>金鼠鼠授勋官护卫（突袭）</summary>
    public static PawnKindDef OA_RK_Assault_B;

    public static PawnGroupKindDef OA_Rk_LargeScaleTrader;

    public static IsolatedPawnGroupMakerDef OA_ResearchSummit_TravellerMaker;

    ///<summary>物质征募队</summary>
    public static TraderKindDef OA_RK_Caravan_TributeCollector;
    ///<summary>科研峰会：集市商人</summary>
    public static TraderKindDef OA_ResearchSummit_FairTrader;
    ///<summary>科研峰会：参会旅行者</summary>
    public static TraderKindDef OA_ResearchSummit_TravellerTrader;

    static OARK_PawnGenerateDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(OARK_PawnGenerateDefOf));
    }
}