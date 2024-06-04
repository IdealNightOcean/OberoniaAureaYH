using HarmonyLib;
using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace OberoniaAurea_PatrickStar;

[HarmonyPatch(typeof(RoyalTitleUtility), "GenerateBestowingCeremonyQuest")]
public static class RoyalTitleUtility_GenerateBestowingCeremonyQuest_Patch
{
	[HarmonyPrefix]
	public static bool Prefix(Pawn pawn, Faction faction)
	{
		Slate slate = new();
		slate.Set("titleHolder", pawn);
		slate.Set("bestowingFaction", faction);
		if (OBDefOf.OMBestowingCeremony.CanRun(slate))
		{
			Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(OBDefOf.OMBestowingCeremony, slate);
			if (quest.root.sendAvailableLetter)
			{
				QuestUtility.SendLetterQuestAvailable(quest);
			}
		}
		return false;
	}
}
