﻿using RimWorld;
using RimWorld.QuestGen;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Grammar;

namespace OberoniaAurea;

public class QuestNode_Root_OABestowingCeremony : QuestNode
{
    public const string QuestTag = "Bestowing";
    protected const int DefenderCount = 8;

    protected bool TryGetCeremonyTarget(Slate slate, out Pawn pawn, out Faction bestowingFaction)
    {
        pawn = null;

        slate.TryGet("bestowingFaction", out bestowingFaction);
        if (OA_MiscUtility.OAFaction == null)
        {
            return false;
        }

        if (slate.TryGet("titleHolder", out pawn) && pawn.Faction != null && pawn.Faction.IsPlayer)
        {
            if (bestowingFaction != null)
            {
                return RoyalTitleUtility.ShouldGetBestowingCeremonyQuest(pawn, bestowingFaction);
            }
            return RoyalTitleUtility.ShouldGetBestowingCeremonyQuest(pawn, out bestowingFaction);
        }
        if (bestowingFaction.IsOAFaction())
        {
            return false;
        }
        pawn = null;
        foreach (Map map in Find.Maps)
        {
            if (!map.IsPlayerHome)
            {
                continue;
            }
            foreach (Pawn allPawn in map.mapPawns.FreeColonistsSpawned)
            {
                if (bestowingFaction != null)
                {
                    return RoyalTitleUtility.ShouldGetBestowingCeremonyQuest(allPawn, bestowingFaction);
                }
                return RoyalTitleUtility.ShouldGetBestowingCeremonyQuest(allPawn, out bestowingFaction);
            }
        }
        bestowingFaction = null;
        return false;
    }
    protected override bool TestRunInt(Slate slate)
    {
        if (!TryGetCeremonyTarget(slate, out _, out var bestowingFaction) || bestowingFaction.HostileTo(Faction.OfPlayer))
        {
            return false;
        }
        QuestGen_Pawns.GetPawnParms parms = default;
        parms.mustBeOfKind = OA_PawnGenerateDefOf.OA_RK_Noble_C;
        parms.canGeneratePawn = true;
        parms.mustBeOfFaction = bestowingFaction;
        if (!QuestGen_Pawns.GetPawnTest(parms, out _))
        {
            return false;
        }
        return true;
    }

    protected override void RunInt()
    {
        if (!ModLister.CheckRoyalty("Bestowing ceremony"))
        {
            return;
        }
        Quest quest = QuestGen.quest;
        Slate slate = QuestGen.slate;
        if (!TryGetCeremonyTarget(slate, out Pawn pawn, out Faction bestowingFaction))
        {
            return;
        }


        RoyalTitleDef titleAwardedWhenUpdating = pawn.royalty.GetTitleAwardedWhenUpdating(bestowingFaction, pawn.royalty.GetFavor(bestowingFaction));
        string tagStr = QuestGenUtility.HardcodedTargetQuestTagWithQuestID("Bestowing");
        string expiredSignal = QuestGenUtility.QuestTagSignal(tagStr, "CeremonyExpired");
        string failedSignal = QuestGenUtility.QuestTagSignal(tagStr, "CeremonyFailed");
        string doneSignal = QuestGenUtility.QuestTagSignal(tagStr, "CeremonyDone");
        string attackedSignal = QuestGenUtility.QuestTagSignal(tagStr, "BeingAttacked");
        string fleeingSignal = QuestGenUtility.QuestTagSignal(tagStr, "Fleeing");
        string awardChangedSignal = QuestGenUtility.QuestTagSignal(tagStr, "TitleAwardedWhenUpdatingChanged");

        Thing thing = QuestGen_Shuttle.GenerateShuttle(bestowingFaction);
        Pawn bestower = quest.GetPawn(new QuestGen_Pawns.GetPawnParms
        {
            mustBeOfKind = OA_PawnGenerateDefOf.OA_RK_Noble_C,
            canGeneratePawn = true,
            mustBeOfFaction = bestowingFaction,
            mustBeWorldPawn = true,
            ifWorldPawnThenMustBeFree = true,
            redressPawn = true
        });
        QuestUtility.AddQuestTag(ref thing.questTags, tagStr);
        QuestUtility.AddQuestTag(ref pawn.questTags, tagStr);

        List<Pawn> shuttleContents = [bestower];
        slate.Set("shuttleContents", shuttleContents);
        slate.Set("shuttle", thing);
        slate.Set("target", pawn);
        slate.Set("bestower", bestower);
        slate.Set("bestowingFaction", bestowingFaction);

        List<Pawn> defenders = [];
        for (int j = 0; j < DefenderCount; j++)
        {
            Pawn defender = quest.GeneratePawn(OA_PawnGenerateDefOf.OA_RK_Guard_Member, bestowingFaction);
            shuttleContents.Add(defender);
            defenders.Add(defender);
        }
        quest.EnsureNotDowned(shuttleContents);
        slate.Set("defenders", defenders);

        thing.TryGetComp<CompShuttle>().requiredPawns = shuttleContents;
        TransportShip transportShip = quest.GenerateTransportShip(TransportShipDefOf.Ship_Shuttle, shuttleContents, thing).transportShip;
        quest.AddShipJob_Arrive(transportShip, null, pawn, null, ShipJobStartMode.Instant, Faction.OfEmpire);
        quest.AddShipJob(transportShip, ShipJobDefOf.Unload);
        quest.AddShipJob_WaitForever(transportShip, leaveImmediatelyWhenSatisfied: true, showGizmos: false, shuttleContents.Cast<Thing>().ToList()).sendAwayIfAnyDespawnedDownedOrDead = [bestower];
        QuestUtility.AddQuestTag(ref transportShip.questTags, tagStr);
        quest.FactionGoodwillChange(bestowingFaction, -5, QuestGenUtility.HardcodedSignalWithQuestID("defenders.Killed"), canSendMessage: true, canSendHostilityLetter: true, getLookTargetFromSignal: true, HistoryEventDefOf.QuestPawnLost);

        QuestPart_BestowingCeremony questPart_BestowingCeremony = new()
        {
            inSignal = slate.Get<string>("inSignal"),
            mapOfPawn = pawn,
            faction = bestowingFaction,
            bestower = bestower,
            target = pawn,
            shuttle = thing,
            questTag = tagStr
        };
        questPart_BestowingCeremony.pawns.Add(bestower);
        quest.AddPart(questPart_BestowingCeremony);

        QuestPart_EscortPawn questPart_EscortPawn = new()
        {
            inSignal = slate.Get<string>("inSignal"),
            escortee = bestower,
            mapOfPawn = pawn,
            faction = bestowingFaction,
            shuttle = thing,
            questTag = tagStr,
            leavingDangerMessage = "MessageBestowingDanger".Translate()
        };
        questPart_EscortPawn.pawns.AddRange(defenders);
        quest.AddPart(questPart_EscortPawn);

        string shuttleKilledSignal = QuestGenUtility.HardcodedSignalWithQuestID("shuttle.Killed");
        quest.FactionGoodwillChange(bestowingFaction, 0, shuttleKilledSignal, canSendMessage: true, canSendHostilityLetter: true, getLookTargetFromSignal: true, HistoryEventDefOf.ShuttleDestroyed, QuestPart.SignalListenMode.OngoingOnly, ensureMakesHostile: true);
        quest.End(QuestEndOutcome.Fail, 0, null, shuttleKilledSignal, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);

        QuestPart_RequirementsToAcceptThroneRoom questPart_RequirementsToAcceptThroneRoom = new()
        {
            faction = bestowingFaction,
            forPawn = pawn,
            forTitle = titleAwardedWhenUpdating
        };
        quest.AddPart(questPart_RequirementsToAcceptThroneRoom);

        QuestPart_RequirementsToAcceptPawnOnColonyMap questPart_RequirementsToAcceptPawnOnColonyMap = new()
        {
            pawn = pawn
        };
        quest.AddPart(questPart_RequirementsToAcceptPawnOnColonyMap);

        QuestPart_RequirementsToAcceptNoDanger questPart_RequirementsToAcceptNoDanger = new()
        {
            mapPawn = pawn,
            dangerTo = bestowingFaction
        };
        quest.AddPart(questPart_RequirementsToAcceptNoDanger);

        quest.AddPart(new QuestPart_OARequirementsToAcceptNoOngoingBestowingCeremony());

        string recruitedSignal = QuestGenUtility.HardcodedSignalWithQuestID("shuttleContents.Recruited");
        string hostileSignal = QuestGenUtility.HardcodedSignalWithQuestID("bestowingFaction.BecameHostileToPlayer");
        quest.Signal(recruitedSignal, delegate
        {
            quest.End(QuestEndOutcome.Fail, 0, null, null, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
        });

        quest.Bestowing_TargetChangedTitle(pawn, bestower, titleAwardedWhenUpdating, awardChangedSignal);
        quest.Letter(LetterDefOf.NegativeEvent, expiredSignal, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, label: "LetterLabelBestowingCeremonyExpired".Translate(), text: "LetterTextBestowingCeremonyExpired".Translate(pawn.Named("TARGET")));

        quest.End(QuestEndOutcome.Fail, 0, null, QuestGenUtility.HardcodedSignalWithQuestID("target.Killed"), QuestPart.SignalListenMode.OngoingOrNotYetAccepted, sendStandardLetter: true);
        quest.End(QuestEndOutcome.Fail, 0, null, QuestGenUtility.HardcodedSignalWithQuestID("bestower.Killed"), QuestPart.SignalListenMode.OngoingOrNotYetAccepted, sendStandardLetter: true);
        quest.End(QuestEndOutcome.Fail, 0, null, expiredSignal);
        quest.End(QuestEndOutcome.Fail, 0, null, hostileSignal, QuestPart.SignalListenMode.OngoingOrNotYetAccepted, sendStandardLetter: true);
        quest.End(QuestEndOutcome.Fail, 0, null, failedSignal, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
        quest.End(QuestEndOutcome.Fail, 0, null, attackedSignal, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
        quest.End(QuestEndOutcome.Fail, 0, null, fleeingSignal, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
        quest.End(QuestEndOutcome.Success, 0, null, doneSignal);

        QuestPart_Choice questPart_Choice = quest.RewardChoice();
        QuestPart_Choice.Choice item2 = new()
        {
            rewards = { new Reward_BestowingCeremony
            {
                targetPawnName = pawn.NameShortColored.Resolve(),
                titleName = titleAwardedWhenUpdating.GetLabelCapFor(pawn),
                awardingFaction = bestowingFaction,
                givePsylink = (titleAwardedWhenUpdating.maxPsylinkLevel > pawn.GetPsylinkLevel()),
                royalTitle = titleAwardedWhenUpdating
            } }
        };
        questPart_Choice.choices.Add(item2);
        List<Rule> list3 =
        [
            .. GrammarUtility.RulesForPawn("pawn", pawn),
            new Rule_String("newTitle", titleAwardedWhenUpdating.GetLabelCapFor(pawn)),
        ];
        QuestGen.AddQuestNameRules(list3);
        List<Rule> list4 =
        [
            .. GrammarUtility.RulesForFaction("faction", bestowingFaction),
            .. GrammarUtility.RulesForPawn("pawn", pawn),
            new Rule_String("newTitle", pawn.royalty.GetTitleAwardedWhenUpdating(bestowingFaction, pawn.royalty.GetFavor(bestowingFaction)).GetLabelFor(pawn)),
        ];
        QuestGen.AddQuestDescriptionRules(list4);
    }
}