using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace OberoniaAurea;

//精神崩溃Patch
[StaticConstructorOnStartup]
[HarmonyPatch(typeof(MentalBreakWorker), nameof(MentalBreakWorker.TryStart))]
public static class MBW_TryStart_Patch
{
    [HarmonyPostfix]
    public static void Postfix(ref bool __result, Pawn pawn)
    {
        if (!__result || !ModsConfig.IdeologyActive || pawn.Ideo is null)
        {
            return;
        }
        Ideo ideo = pawn.Ideo;

        if (ideo.HasPrecept(OARK_PreceptDefOf.OA_RK_MentalBreakProbability_Low))
        {
            MentalBreakProbability(pawn: pawn,
                                   chance: 0.5f,
                                   thought: OARK_ThoughtDefOf.OA_RK_ResponsibilityConstraints,
                                   message: "OA_ResponsibilityConstraints");
        }
        else if (ideo.HasPrecept(OARK_PreceptDefOf.OA_RK_MentalBreakProbability_Atonement))
        {
            MentalBreakProbability(pawn: pawn,
                                   chance: 0.8f,
                                   thought: OARK_ThoughtDefOf.OA_RK_Atonement,
                                   message: "OA_Atonement");
        }
    }

    private static void MentalBreakProbability(Pawn pawn, float chance, ThoughtDef thought, string message)
    {
        //有戒律崩溃后概率恢复
        if (Rand.Chance(chance))
        {
            //拥有对应心情修正的情况下再次崩溃不会触发判定
            Thought_Memory tm = pawn.needs.mood?.thoughts?.memories?.GetFirstMemoryOfDef(thought);
            if (tm is not null)
            {
                return;
            }
            pawn.mindState.mentalStateHandler.Reset();

            pawn.needs.mood?.thoughts?.memories?.TryGainMemory(thought);
            Messages.Message(message.Translate(pawn.Named("NAME")), MessageTypeDefOf.PositiveEvent);
        }
    }

}