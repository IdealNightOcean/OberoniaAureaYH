using RimWorld;
using Verse;

namespace OberoniaAurea;

[DefOf]
public static class OARK_ModDefOf
{
    ///<summary>
    ///重力陷阱爆炸
    ///</summary>
    [MayRequireOdyssey]
    public static EffecterDef OARK_Effect_GravTrapIED;

    ///<summary>
    ///金鼠玩家派系
    ///</summary>
    public static FactionDef OA_RK_PlayerFaction;

    ///<summary>
    ///金鼠派系
    ///</summary>
    public static FactionDef OA_RK_Faction;

    ///<summary>
    ///旅鼠派系
    ///</summary>
    [MayRequire("OARK.RatkinFaction.GeneExpand")]
    public static FactionDef Rakinia_TravelRatkin;

    [MayRequireOdyssey]
    public static FleckDef OARK_Fleck_GravBomb;

    ///<summary>
    ///军事部署
    ///</summary>
    public static GameConditionDef OA_MilitaryDeployment;

    ///<summary>
    ///引力扭曲轰炸
    ///</summary>
    [MayRequireOdyssey]
    public static GameConditionDef OARK_GravityDistortionBomb;

    [MayRequireBiotech]
    public static GeneDef RK_OA_gene_Social;

    public static JobDef OARK_Job_TalkWithProspectingLeader;

    ///<summary>
    ///整修电路调控
    ///</summary>
    public static JobDef OA_RK_RepairCircuitRegulator;

    [MayRequireOdyssey]
    public static JobDef OARK_Job_SailorInvitation;

    [MayRequireOdyssey]
    public static JobDef OARK_Job_Hack;

    ///<summary>
    ///和谐团体
    ///</summary>
    [MayRequireIdeology]
    public static MemeDef OA_RK_meme_Friendly;

    public static StorytellerDef Begonia_Oberonia_Aurea;

    [MayRequireOdyssey]
    public static SoundDef OARK_Sound_GravBomb;

    [MayRequireOdyssey]
    public static SketchResolverDef OARK_Sketch_CrashedGravship;

    static OARK_ModDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(OARK_ModDefOf));
    }
}