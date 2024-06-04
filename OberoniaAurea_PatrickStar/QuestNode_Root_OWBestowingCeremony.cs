using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.QuestGen;
using Verse;
using Verse.Grammar;

namespace OberoniaAurea_PatrickStar;

internal class QuestNode_Root_OWBestowingCeremony : QuestNode
{
	public const string QuestTag = "Bestowing";

	private bool TryGetCeremonyTarget(Slate slate, out Pawn pawn, out Faction bestowingFaction)
	{
		slate.TryGet<Faction>("bestowingFaction", out bestowingFaction);
		if (slate.TryGet<Pawn>("titleHolder", out pawn) && pawn.Faction != null && pawn.Faction.IsPlayer)
		{
			if (bestowingFaction != null)
			{
				return RoyalTitleUtility.ShouldGetBestowingCeremonyQuest(pawn, bestowingFaction);
			}
			return RoyalTitleUtility.ShouldGetBestowingCeremonyQuest(pawn, out bestowingFaction);
		}
		pawn = null;
		foreach (Map map in Find.Maps)
		{
			if (!map.IsPlayerHome)
			{
				continue;
			}
			foreach (Pawn allPawn in map.mapPawns.AllPawns)
			{
				if (allPawn.Faction != null && allPawn.Faction.IsPlayer)
				{
					if (bestowingFaction != null)
					{
						return RoyalTitleUtility.ShouldGetBestowingCeremonyQuest(allPawn, bestowingFaction);
					}
					return RoyalTitleUtility.ShouldGetBestowingCeremonyQuest(allPawn, out bestowingFaction);
				}
			}
		}
		bestowingFaction = null;
		return false;
	}

	protected override void RunInt()
	{
		if (!ModLister.CheckRoyalty("Bestowing ceremony"))
		{
			return;
		}
		Quest quest = QuestGen.quest;
		Slate slate = QuestGen.slate;
		if (!TryGetCeremonyTarget(QuestGen.slate, out var pawn, out var bestowingFaction))
		{
			return;
		}
		RoyalTitleDef titleAwardedWhenUpdating = pawn.royalty.GetTitleAwardedWhenUpdating(bestowingFaction, pawn.royalty.GetFavor(bestowingFaction));
		string text = QuestGenUtility.HardcodedTargetQuestTagWithQuestID("Bestowing");
		string text2 = QuestGenUtility.QuestTagSignal(text, "CeremonyExpired");
		string text3 = QuestGenUtility.QuestTagSignal(text, "CeremonyFailed");
		string text4 = QuestGenUtility.QuestTagSignal(text, "CeremonyDone");
		string text5 = QuestGenUtility.QuestTagSignal(text, "BeingAttacked");
		string text6 = QuestGenUtility.QuestTagSignal(text, "Fleeing");
		string inSignal = QuestGenUtility.QuestTagSignal(text, "TitleAwardedWhenUpdatingChanged");
		Thing thing = QuestGen_Shuttle.GenerateShuttle(bestowingFaction);
		Pawn pawn2 = new Pawn();
		pawn2 = ((bestowingFaction != Find.FactionManager.FirstFactionOfDef(OBDefOf.OA_RK_Faction)) ? quest.GetPawn(new QuestGen_Pawns.GetPawnParms
		{
			mustBeOfKind = PawnKindDefOf.Empire_Royal_Bestower,
			canGeneratePawn = true,
			mustBeOfFaction = bestowingFaction,
			mustBeWorldPawn = true,
			ifWorldPawnThenMustBeFree = true,
			redressPawn = true
		}) : quest.GetPawn(new QuestGen_Pawns.GetPawnParms
		{
			mustBeOfKind = OBDefOf.OA_RK_Noble_C,
			canGeneratePawn = true,
			mustBeOfFaction = bestowingFaction,
			mustBeWorldPawn = true,
			ifWorldPawnThenMustBeFree = true,
			redressPawn = true
		}));
		QuestUtility.AddQuestTag(ref thing.questTags, text);
		QuestUtility.AddQuestTag(ref pawn.questTags, text);
		ThingOwner<Thing> innerContainer = pawn2.inventory.innerContainer;
		for (int num = innerContainer.Count - 1; num >= 0; num--)
		{
			if (innerContainer[num].def == ThingDefOf.PsychicAmplifier)
			{
				Thing thing2 = innerContainer[num];
				innerContainer.RemoveAt(num);
				thing2.Destroy();
			}
		}
		for (int i = 0; i < 2; i++)
		{
			innerContainer.TryAdd(ThingMaker.MakeThing(ThingDefOf.PsychicAmplifier), 1);
		}
		List<Pawn> list = new List<Pawn>();
		list.Add(pawn2);
		slate.Set("shuttleContents", list);
		slate.Set("shuttle", thing);
		slate.Set("target", pawn);
		slate.Set("bestower", pawn2);
		slate.Set("bestowingFaction", bestowingFaction);
		List<Pawn> list2 = new List<Pawn>();
		if (bestowingFaction == Find.FactionManager.FirstFactionOfDef(OBDefOf.OA_RK_Faction))
		{
			for (int j = 0; j < 8; j++)
			{
				Pawn pawn3 = new Pawn();
				pawn3 = ((j >= 2) ? quest.GeneratePawn(OBDefOf.OA_RK_Guard_Member, bestowingFaction) : quest.GeneratePawn(OBDefOf.OA_RK_Assault_B, bestowingFaction));
				list.Add(pawn3);
				list2.Add(pawn3);
			}
		}
		else
		{
			for (int k = 0; k < 6; k++)
			{
				Pawn item = quest.GeneratePawn(PawnKindDefOf.Empire_Fighter_Janissary, bestowingFaction);
				list.Add(item);
				list2.Add(item);
			}
		}
		quest.EnsureNotDowned(list);
		slate.Set("defenders", list2);
		thing.TryGetComp<CompShuttle>().requiredPawns = list;
		TransportShip transportShip = quest.GenerateTransportShip(TransportShipDefOf.Ship_Shuttle, list, thing).transportShip;
		if (bestowingFaction == Find.FactionManager.FirstFactionOfDef(OBDefOf.OA_RK_Faction))
		{
			quest.AddShipJob_Arrive(transportShip, pawn.MapHeld.Parent, pawn, null, ShipJobStartMode.Instant, Find.FactionManager.FirstFactionOfDef(OBDefOf.OA_RK_Faction));
		}
		else
		{
			quest.AddShipJob_Arrive(transportShip, pawn.MapHeld.Parent, pawn, null, ShipJobStartMode.Instant, Faction.OfEmpire);
		}
		quest.AddShipJob(transportShip, ShipJobDefOf.Unload);
		quest.AddShipJob_WaitForever(transportShip, leaveImmediatelyWhenSatisfied: true, showGizmos: false, list.Cast<Thing>().ToList()).sendAwayIfAnyDespawnedDownedOrDead = new List<Thing> { pawn2 };
		QuestUtility.AddQuestTag(ref transportShip.questTags, text);
		quest.FactionGoodwillChange(bestowingFaction, -5, QuestGenUtility.HardcodedSignalWithQuestID("defenders.Killed"), canSendMessage: true, canSendHostilityLetter: true, getLookTargetFromSignal: true, HistoryEventDefOf.QuestPawnLost);
		QuestPart_BestowingCeremony questPart_BestowingCeremony = new QuestPart_BestowingCeremony();
		questPart_BestowingCeremony.inSignal = QuestGen.slate.Get<string>("inSignal");
		questPart_BestowingCeremony.pawns.Add(pawn2);
		questPart_BestowingCeremony.mapOfPawn = pawn;
		questPart_BestowingCeremony.faction = pawn2.Faction;
		questPart_BestowingCeremony.bestower = pawn2;
		questPart_BestowingCeremony.target = pawn;
		questPart_BestowingCeremony.shuttle = thing;
		questPart_BestowingCeremony.questTag = text;
		quest.AddPart(questPart_BestowingCeremony);
		QuestPart_EscortPawn questPart_EscortPawn = new QuestPart_EscortPawn();
		questPart_EscortPawn.inSignal = QuestGen.slate.Get<string>("inSignal");
		questPart_EscortPawn.escortee = pawn2;
		questPart_EscortPawn.pawns.AddRange(list2);
		questPart_EscortPawn.mapOfPawn = pawn;
		questPart_EscortPawn.faction = pawn2.Faction;
		questPart_EscortPawn.shuttle = thing;
		questPart_EscortPawn.questTag = text;
		questPart_EscortPawn.leavingDangerMessage = "MessageBestowingDanger".Translate();
		quest.AddPart(questPart_EscortPawn);
		string text7 = QuestGenUtility.HardcodedSignalWithQuestID("shuttle.Killed");
		quest.FactionGoodwillChange(bestowingFaction, 0, text7, canSendMessage: true, canSendHostilityLetter: true, getLookTargetFromSignal: true, HistoryEventDefOf.ShuttleDestroyed, QuestPart.SignalListenMode.OngoingOnly, ensureMakesHostile: true);
		QuestGen_End.End(quest, QuestEndOutcome.Fail, 0, (Faction)null, text7, QuestPart.SignalListenMode.OngoingOnly, true);
		QuestPart_RequirementsToAcceptThroneRoom questPart_RequirementsToAcceptThroneRoom = new QuestPart_RequirementsToAcceptThroneRoom();
		questPart_RequirementsToAcceptThroneRoom.faction = bestowingFaction;
		questPart_RequirementsToAcceptThroneRoom.forPawn = pawn;
		questPart_RequirementsToAcceptThroneRoom.forTitle = titleAwardedWhenUpdating;
		quest.AddPart(questPart_RequirementsToAcceptThroneRoom);
		QuestPart_RequirementsToAcceptPawnOnColonyMap questPart_RequirementsToAcceptPawnOnColonyMap = new QuestPart_RequirementsToAcceptPawnOnColonyMap();
		questPart_RequirementsToAcceptPawnOnColonyMap.pawn = pawn;
		quest.AddPart(questPart_RequirementsToAcceptPawnOnColonyMap);
		QuestPart_RequirementsToAcceptNoDanger questPart_RequirementsToAcceptNoDanger = new QuestPart_RequirementsToAcceptNoDanger();
		questPart_RequirementsToAcceptNoDanger.mapParent = pawn.Map.Parent;
		questPart_RequirementsToAcceptNoDanger.dangerTo = bestowingFaction;
		quest.AddPart(questPart_RequirementsToAcceptNoDanger);
		quest.AddPart(new QuestPart_RequirementsToAcceptNoOngoingBestowingCeremonys());
		string inSignal2 = QuestGenUtility.HardcodedSignalWithQuestID("shuttleContents.Recruited");
		string text8 = QuestGenUtility.HardcodedSignalWithQuestID("bestowingFaction.BecameHostileToPlayer");
		quest.Signal(inSignal2, delegate
		{
			QuestGen_End.End(quest, QuestEndOutcome.Fail, 0, (Faction)null, (string)null, QuestPart.SignalListenMode.OngoingOnly, true);
		});
		quest.Bestowing_TargetChangedTitle(pawn, pawn2, titleAwardedWhenUpdating, inSignal);
		quest.Letter(LetterDefOf.NegativeEvent, text2, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, label: "LetterLabelBestowingCeremonyExpired".Translate(), text: "LetterTextBestowingCeremonyExpired".Translate(pawn.Named("TARGET")));
		QuestGen_End.End(quest, QuestEndOutcome.Fail, 0, (Faction)null, QuestGenUtility.HardcodedSignalWithQuestID("target.Killed"), QuestPart.SignalListenMode.OngoingOrNotYetAccepted, true);
		QuestGen_End.End(quest, QuestEndOutcome.Fail, 0, (Faction)null, QuestGenUtility.HardcodedSignalWithQuestID("bestower.Killed"), QuestPart.SignalListenMode.OngoingOrNotYetAccepted, true);
		QuestGen_End.End(quest, QuestEndOutcome.Fail, 0, (Faction)null, text2, QuestPart.SignalListenMode.OngoingOnly, false);
		QuestGen_End.End(quest, QuestEndOutcome.Fail, 0, (Faction)null, text8, QuestPart.SignalListenMode.OngoingOrNotYetAccepted, true);
		QuestGen_End.End(quest, QuestEndOutcome.Fail, 0, (Faction)null, text3, QuestPart.SignalListenMode.OngoingOnly, true);
		QuestGen_End.End(quest, QuestEndOutcome.Fail, 0, (Faction)null, text5, QuestPart.SignalListenMode.OngoingOnly, true);
		QuestGen_End.End(quest, QuestEndOutcome.Fail, 0, (Faction)null, text6, QuestPart.SignalListenMode.OngoingOnly, true);
		QuestGen_End.End(quest, QuestEndOutcome.Success, 0, (Faction)null, text4, QuestPart.SignalListenMode.OngoingOnly, false);
		QuestPart_Choice questPart_Choice = quest.RewardChoice();
		QuestPart_Choice.Choice item2 = new QuestPart_Choice.Choice
		{
			rewards = { (Reward)new Reward_BestowingCeremony
			{
				targetPawnName = pawn.NameShortColored.Resolve(),
				titleName = titleAwardedWhenUpdating.GetLabelCapFor(pawn),
				awardingFaction = bestowingFaction,
				givePsylink = (titleAwardedWhenUpdating.maxPsylinkLevel > pawn.GetPsylinkLevel()),
				royalTitle = titleAwardedWhenUpdating
			} }
		};
		questPart_Choice.choices.Add(item2);
		List<Rule> list3 = new List<Rule>();
		list3.AddRange(GrammarUtility.RulesForPawn("pawn", pawn));
		list3.Add(new Rule_String("newTitle", titleAwardedWhenUpdating.GetLabelCapFor(pawn)));
		QuestGen.AddQuestNameRules(list3);
		List<Rule> list4 = new List<Rule>();
		list4.AddRange(GrammarUtility.RulesForFaction("faction", bestowingFaction));
		list4.AddRange(GrammarUtility.RulesForPawn("pawn", pawn));
		list4.Add(new Rule_String("newTitle", pawn.royalty.GetTitleAwardedWhenUpdating(bestowingFaction, pawn.royalty.GetFavor(bestowingFaction)).GetLabelFor(pawn)));
		list4.Add(new Rule_String("psylinkLevel", titleAwardedWhenUpdating.maxPsylinkLevel.ToString()));
		QuestGen.AddQuestDescriptionRules(list4);
	}

	protected override bool TestRunInt(Slate slate)
	{
		if (!TryGetCeremonyTarget(slate, out var pawn, out var bestowingFaction) || bestowingFaction.HostileTo(Faction.OfPlayer))
		{
			return false;
		}
		if (!pawn.Spawned)
		{
			return false;
		}
		QuestGen_Pawns.GetPawnParms parms = default(QuestGen_Pawns.GetPawnParms);
		parms.mustBeOfKind = PawnKindDefOf.Empire_Royal_Bestower;
		parms.canGeneratePawn = true;
		parms.mustBeOfFaction = bestowingFaction;
		if (!QuestGen_Pawns.GetPawnTest(parms, out var _))
		{
			return false;
		}
		return true;
	}
}
