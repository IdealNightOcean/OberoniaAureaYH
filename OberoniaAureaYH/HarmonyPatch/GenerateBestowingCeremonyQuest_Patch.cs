using HarmonyLib;
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
            if (OA_QuestScriptDefOf.OA_BestowingCeremony.CanRun(slate))
            {
                Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(OA_QuestScriptDefOf.OA_BestowingCeremony, slate);
                if (quest.root.sendAvailableLetter)
                {
                    QuestUtility.SendLetterQuestAvailable(quest);
                }
                return false;
            }
        }
        return true;
    }
}
