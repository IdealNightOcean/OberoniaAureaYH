using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace OberoniaAurea;

//精神崩溃Patch
[StaticConstructorOnStartup]
[HarmonyPatch(typeof(MentalBreakWorker), "TryStart")]
public static class MBW_TryStart_Patch
{
    [HarmonyPostfix]
    public static void Postfix(ref bool __result, Pawn pawn)
    {
        if (!__result || !ModsConfig.IdeologyActive)
        {
            return;
        }
        if (pawn.Ideo == null)
        {
            return;
        }

        if (pawn.Ideo.HasPrecept(OberoniaAureaYHDefOf.OA_RK_MentalBreakProbability_Low))
        {
            MentalBreakProbability_Low(pawn);
        }
        else if (pawn.Ideo.HasPrecept(OberoniaAureaYHDefOf.OA_RK_MentalBreakProbability_Atonement))
        {
            MentalBreakProbability_Atonement(pawn);
        }

    }
    private static void MentalBreakProbability_Low(Pawn pawn)
    {
        //有戒律崩溃后50%概率恢复
        if (Rand.Bool)
        {
            //拥有责任感的心情修正的情况下再次崩溃不会触发判定
            Thought_Memory tm = pawn.needs.mood?.thoughts?.memories?.GetFirstMemoryOfDef(OA_PawnInfoDefOf.OA_RK_ResponsibilityConstraints);
            if (tm != null)
            {
                return;
            }
            pawn.mindState.mentalStateHandler.Reset();
            Messages.Message("OA_ResponsibilityConstraints".Translate(pawn.Named("NAME")), MessageTypeDefOf.PositiveEvent);
            pawn.needs.mood?.thoughts?.memories?.TryGainMemory(OA_PawnInfoDefOf.OA_RK_ResponsibilityConstraints);
        }
    }
    private static void MentalBreakProbability_Atonement(Pawn pawn)
    {
        //有戒律崩溃后80%概率恢复
        if (Rand.Value < 0.8)
        {
            // 拥有赎罪的心情修正的情况下再次崩溃不会触发判定
            Thought_Memory tm = pawn.needs.mood?.thoughts?.memories?.GetFirstMemoryOfDef(OA_PawnInfoDefOf.OA_RK_Atonement);
            if (tm != null)
            {
                return;
            }
            pawn.mindState.mentalStateHandler.Reset();
            Messages.Message("OA_Atonement".Translate(pawn.Named("NAME")), MessageTypeDefOf.PositiveEvent);
            pawn.needs.mood?.thoughts?.memories?.TryGainMemory(OA_PawnInfoDefOf.OA_RK_Atonement);
        }

    }


}