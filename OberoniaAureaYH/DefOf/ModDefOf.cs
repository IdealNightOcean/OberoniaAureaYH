using RimWorld;
using Verse;

namespace OberoniaAurea;

[DefOf]
public static class OARK_ModDefOf
{
    [MayRequireOdyssey]
    ///<summary>重力陷阱爆炸</summary>
    public static EffecterDef OARK_Effect_GravTrapIED;

    ///<summary>金鼠玩家派系</summary>
    public static FactionDef OA_RK_PlayerFaction;
    ///<summary>金鼠鼠派系</summary>
    public static FactionDef OA_RK_Faction;
    [MayRequire("OARK.RatkinFaction.GeneExpand")]
    ///<summary>旅鼠派系</summary>
    public static FactionDef Rakinia_TravelRatkin;

    [MayRequireOdyssey]
    public static FleckDef OARK_Fleck_GravBomb;

    ///<summary>军事部署</summary>
    public static GameConditionDef OA_MilitaryDeployment;
    [MayRequireOdyssey]
    ///<summary>引力扭曲轰炸</summary>
    public static GameConditionDef OARK_GravityDistortionBomb;

    [MayRequireBiotech]
    public static GeneDef RK_OA_gene_Social;

    public static JobDef OARK_Job_TalkWithProspectingLeader;
    ///<summary>整修电路调控</summary>
    public static JobDef OA_RK_RepairCircuitRegulator;
    [MayRequireOdyssey]
    public static JobDef OARK_Job_SailorInvitation;
    [MayRequireOdyssey]
    public static JobDef OARK_Job_Hack;

    [MayRequireIdeology]
    ///<summary>和谐团体</summary>
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