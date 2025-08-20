using HarmonyLib;
using OberoniaAurea_Frame;
using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace OberoniaAurea;

//授勋仪式创建Patch

[StaticConstructorOnStartup]
[HarmonyPatch(typeof(RoyalTitleUtility), "GenerateBestowingCeremonyQuest")]
public static class GenerateBestowingCeremonyQuest_Patch
{
    [HarmonyPrefix]
    public static bool Prefix(Pawn pawn, Faction faction)
    {
        if (faction.IsOAFaction())
        {
            Slate slate = new();
            slate.Set("titleHolder", pawn);
            slate.Set("bestowingFaction", faction);
            return !OAFrame_QuestUtility.TryGenerateQuestAndMakeAvailable(out _, OARK_QuestScriptDefOf.OA_BestowingCeremony, slate);
        }
        return true;
    }
}
