using RimWorld;
using Verse;

namespace OberoniaAurea;

[DefOf]
public static class OARK_ModDefOf
{
    [MayRequireOdyssey]
    public static EffecterDef OARK_Effect_GravTrapIED; //重力陷阱爆炸

    public static FactionDef OA_RK_PlayerFaction; //金鼠玩家派系
    public static FactionDef OA_RK_Faction; //金鼠鼠派系
    [MayRequire("OARK.RatkinFaction.GeneExpand")]
    public static FactionDef Rakinia_TravelRatkin; //旅鼠派系

    [MayRequireOdyssey]
    public static FleckDef OARK_Fleck_GravBomb;

    public static GameConditionDef OA_MilitaryDeployment; //军事部署
    [MayRequireOdyssey]
    public static GameConditionDef OARK_GravityDistortionBomb; //引力扭曲轰炸

    [MayRequireBiotech]
    public static GeneDef RK_OA_gene_Social;

    public static JobDef OARK_Job_TalkWithProspectingLeader;
    public static JobDef OA_RK_RepairCircuitRegulator; //整修电路调控
    [MayRequireOdyssey]
    public static JobDef OARK_Job_SailorInvitation;
    [MayRequireOdyssey]
    public static JobDef OARK_Job_Hack;

    [MayRequireIdeology]
    public static MemeDef OA_RK_meme_Friendly; //和谐团体

    [MayRequireOdyssey]
    public static RulePackDef OARK_PackScienceShipName;
    [MayRequireOdyssey]
    public static RulePackDef OARK_RulePackSalutationText;
    [MayRequireOdyssey]
    public static RulePackDef OARK_RulePackProgressText;

    public static StorytellerDef Begonia_Oberonia_Aurea;
    public static SoundDef OARK_Sound_GravBomb;
    [MayRequireOdyssey]
    public static SketchResolverDef OARK_Sketch_CrashedGravship;

    static OARK_ModDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(OARK_ModDefOf));
    }
}