using RimWorld;
using Verse;

namespace OberoniaAurea;

[DefOf]
public static class OARK_ModDefOf
{
    public static FactionDef OA_RK_Faction; //金鼠鼠派系
    [MayRequire("OARK.RatkinFaction.GeneExpand")]
    public static FactionDef Rakinia_TravelRatkin; //金鼠鼠派系

    public static GameConditionDef OA_MilitaryDeployment; //军事部署
    [MayRequireOdyssey]
    public static GameConditionDef OARK_GravityDistortionBomb; //引力扭曲轰炸

    [MayRequireBiotech]
    public static GeneDef RK_OA_gene_Social;

    public static JobDef OA_RK_RepairCircuitRegulator; //整修电路调控
    [MayRequireOdyssey]
    public static JobDef OARK_Job_SailorInvitation;
    [MayRequireOdyssey]
    public static JobDef OARK_Job_Hack;

    [MayRequireIdeology]
    public static MemeDef OA_RK_meme_Friendly; //和谐团体

    [MayRequireOdyssey]
    public static RulePackDef OARK_PackScienceShipName;

    public static StorytellerDef Begonia_Oberonia_Aurea;

    static OARK_ModDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(OARK_ModDefOf));
    }
}