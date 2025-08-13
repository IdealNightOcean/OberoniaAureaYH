using OberoniaAurea_Frame;
using RimWorld;
using Verse;

namespace OberoniaAurea;

[DefOf]
public static class OARK_PawnGenerateDefOf
{
    public static PawnKindDef OA_RK_Court_Member_Exploration; //勘探队员

    public static PawnKindDef OA_RK_Traveller;
    [MayRequireRoyalty]
    public static PawnKindDef OA_RK_Noble_C; //金鼠鼠授勋官
    [MayRequireRoyalty]
    public static PawnKindDef OA_RK_Guard_Member;  //金鼠鼠授勋官护卫
    [MayRequireRoyalty]
    public static PawnKindDef OA_RK_Assault_B;  //金鼠鼠授勋官护卫（突袭）

    public static PawnGroupKindDef OA_Rk_LargeScaleTrader;

    public static IsolatedPawnGroupMakerDef OA_ResearchSummit_TravellerMaker;

    public static TraderKindDef OA_RK_Caravan_TributeCollector; //物质征募队
    public static TraderKindDef OA_ResearchSummit_FairTrader; //科研峰会：集市商人
    public static TraderKindDef OA_ResearchSummit_TravellerTrader; //科研峰会：参会旅行者

    static OARK_PawnGenerateDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(OARK_PawnGenerateDefOf));
    }
}