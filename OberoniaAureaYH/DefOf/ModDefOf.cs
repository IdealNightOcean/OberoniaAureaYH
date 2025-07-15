using RimWorld;
using Verse;

namespace OberoniaAurea;

[DefOf]
public static class OARK_ModDefOf
{
    public static FactionDef OA_RK_Faction; //金鼠鼠派系

    public static GameConditionDef OA_MilitaryDeployment; //军事部署

    [MayRequireBiotech]
    public static GeneDef RK_OA_gene_Social; //

    public static IncidentDef OARK_LargeScaleTraderArrival;
    public static IncidentDef OARatkin_NewYearEvent; //新年慰问

    public static JobDef OA_RK_RepairCircuitRegulator; //整修电路调控OARK_Job_TalkWithProspectingLeader
    public static JobDef OARK_Job_TalkWithProspectingLeader;

    public static LetterDef OA_RK_AcceptJoinerViewInfo;

    [MayRequireIdeology]
    public static MemeDef OA_RK_meme_Friendly; //和谐团体

    public static StorytellerDef Begonia_Oberonia_Aurea;

    [MayRequireIdeology]
    public static ThoughtDef OA_RK_Atonement;
    [MayRequireIdeology]
    public static ThoughtDef OA_RK_ResponsibilityConstraints;

    static OARK_ModDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(OARK_ModDefOf));
    }
}