using RimWorld;
using RimWorld.QuestGen;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Grammar;

namespace OberoniaAurea;

public class QuestNode_Root_OABestowingCeremony : QuestNode
{
    public const string QuestTag = "Bestowing";
    protected const int DefenderGuardCount = 6;
    protected const int DefenderAssaultCount = 2;

    protected bool TryGetCeremonyTarget(Slate slate, out Pawn pawn, out Faction bestowingFaction)
    {
        pawn = null;

        slate.TryGet("bestowingFaction", out bestowingFaction);
        if (ModUtility.OAFaction is null) //金鸢尾兰派系判定
        {
            return false;
        }

        if (slate.TryGet("titleHolder", out pawn) && pawn.Faction.IsPlayerSafe())
        {
            if (bestowingFaction is not null)
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
                if (bestowingFaction is not null)
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
        if (!TryGetCeremonyTarget(slate, out _, out Faction bestowingFaction) || bestowingFaction.HostileTo(Faction.OfPlayer))
        {
            return false;
        }
        QuestGen_Pawns.GetPawnParms parms = default;
        parms.mustBeOfKind = OARK_PawnGenerateDefOf.OA_RK_Noble_C; //授勋官
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
            mustBeOfKind = OARK_PawnGenerateDefOf.OA_RK_Noble_C,
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
        for (int i = 0; i < DefenderGuardCount; i++)
        {
            //金鼠鼠授勋官护卫
            Pawn defender = quest.GeneratePawn(OARK_PawnGenerateDefOf.OA_RK_Guard_Member, bestowingFaction);
            shuttleContents.Add(defender);
            defenders.Add(defender);
        }
        for (int i = 0; i < DefenderAssaultCount; i++)
        {
            //金鼠鼠授勋官护卫（突袭）
            Pawn defender = quest.GeneratePawn(OARK_PawnGenerateDefOf.OA_RK_Assault_B, bestowingFaction);
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

        //授勋仪式（part有改动）
        QuestPart_OABestowingCeremony questPart_OABestowingCeremony = new()
        {
            inSignal = slate.Get<string>("inSignal"),
            mapOfPawn = pawn,
            faction = bestowingFaction,
            bestower = bestower,
            target = pawn,
            shuttle = thing,
            questTag = tagStr
        };
        questPart_OABestowingCeremony.pawns.Add(bestower);
        quest.AddPart(questPart_OABestowingCeremony);

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

        //授勋仪式不可同时进行（金鼠鼠互斥）
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
        QuestPart_Choice.Choice rewardChoice = new()
        {
            rewards = { new Reward_BestowingCeremony
            {
                targetPawnName = pawn.NameShortColored.Resolve(),
                titleName = titleAwardedWhenUpdating.GetLabelCapFor(pawn),
                awardingFaction = bestowingFaction,
                givePsylink = false, //金鼠授勋不给启灵
                royalTitle = titleAwardedWhenUpdating
            } }
        };
        questPart_Choice.choices.Add(rewardChoice);
        List<Rule> newTitleLabel =
        [
            .. GrammarUtility.RulesForPawn("pawn", pawn),
            new Rule_String("newTitle", titleAwardedWhenUpdating.GetLabelCapFor(pawn)),
        ];
        QuestGen.AddQuestNameRules(newTitleLabel);
        List<Rule> newTitleAward =
        [
            .. GrammarUtility.RulesForFaction("faction", bestowingFaction),
            .. GrammarUtility.RulesForPawn("pawn", pawn),
            new Rule_String("newTitle", pawn.royalty.GetTitleAwardedWhenUpdating(bestowingFaction, pawn.royalty.GetFavor(bestowingFaction)).GetLabelFor(pawn)),
        ];
        QuestGen.AddQuestDescriptionRules(newTitleAward);
    }
}