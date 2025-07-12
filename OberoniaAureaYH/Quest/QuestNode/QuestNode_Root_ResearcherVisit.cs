using OberoniaAurea_Frame;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace OberoniaAurea;
public class QuestNode_Root_ResearcherVisit : QuestNode_Root_RefugeeBase
{
    protected override IntRange LodgerCount => new(2, 3);

    private const int QuestDurationDays = 4;
    private const int QuestDurationTicks = QuestDurationDays * 60000;
    private const int ArrivalDelayTicks = 120000;
    private static readonly IntRange IntellectualSkill = new(8, 18);

    protected override void RunInt()
    {
        Faction faction = GetOrGenerateFaction();

        Quest quest = QuestGen.quest;
        Slate slate = QuestGen.slate;
        Map map = QuestGen_Get.GetMap();
        int lodgerCount = LodgerCount.RandomInRange;
        string lodgerRecruitedSignal = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.Recruited");
        string lodgerArrestedSignal = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.Arrested");
        string lodgerBecameMutantSignal = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.BecameMutant");
        string lodgerArrestedOrRecruited = QuestGen.GenerateNewSignal("Lodger_ArrestedOrRecruited");
        quest.AnySignal([lodgerRecruitedSignal, lodgerArrestedSignal], null, [lodgerArrestedOrRecruited]);

        List<Pawn> pawns = GeneratePawns(lodgerCount, faction, map, quest, lodgerRecruitedSignal);
        faction.leader = pawns.First();
        Pawn asker = pawns.First();

        QuestPart_ExtraFaction questPart_ExtraFaction = quest.ExtraFaction(faction, pawns, ExtraFactionType.MiniFaction, areHelpers: false, [lodgerRecruitedSignal, lodgerBecameMutantSignal]);

        string lodgerArrival = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.Arrival");
        quest.Delay(ArrivalDelayTicks, delegate
        {
            quest.PawnsArrive(pawns, null, map.Parent, null, joinPlayer: true, null, "[lodgersArriveLetterLabel]", "[lodgersArriveLetterText]");
        }, null, null, lodgerArrival);

        quest.SetAllApparelLocked(pawns);

        SetAward(quest);

        QuestPart_OARefugeeInteractions questPart_ResearcherInteractions = WoundedTravelerInteractions(faction, map.Parent);
        questPart_ResearcherInteractions.inSignalArrested = lodgerArrestedSignal;
        questPart_ResearcherInteractions.inSignalRecruited = lodgerRecruitedSignal;
        questPart_ResearcherInteractions.pawns.AddRange(pawns);
        quest.AddPart(questPart_ResearcherInteractions);

        //准时离开
        quest.Delay(QuestDurationTicks, delegate
        {
            quest.SignalPassWithFaction(faction, null, delegate
            {
                quest.Letter(LetterDefOf.PositiveEvent, null, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, "[lodgersLeavingLetterText]", null, "[lodgersLeavingLetterLabel]");
            });
            quest.Leave(pawns, null, sendStandardLetter: false, leaveOnCleanup: false, lodgerArrestedOrRecruited, wakeUp: true);
        }, lodgerArrival, null, null, reactivatable: false, null, null, isQuestTimeout: false, "GuestsDepartsIn".Translate(), "GuestsDepartsOn".Translate(), "QuestDelay");

        SetQuestEndComp(quest, questPart_ResearcherInteractions, pawns, faction, lodgerCount);

        slate.Set("map", map);
        slate.Set("faction", faction);
        slate.Set("questDurationTicks", QuestDurationTicks);
        slate.Set("arrivalDelayTicks", ArrivalDelayTicks);
        slate.Set("lodgerCount", lodgerCount);
        slate.Set("lodgersCountMinusOne", lodgerCount - 1);
        slate.Set("lodgers", pawns);
        slate.Set("asker", asker);
    }
    protected override List<Pawn> GeneratePawns(int lodgerCount, Faction faction, Map map, Quest quest, string lodgerRecruitedSignal = null)
    {
        List<Pawn> pawns = [];
        for (int i = 0; i < lodgerCount; i++)
        {
            DevelopmentalStage developmentalStages = DevelopmentalStage.Adult;
            Pawn pawn = quest.GeneratePawn(PawnKindDefOf.Empire_Common_Lodger, faction, allowAddictions: false, null, 0f, mustBeCapableOfViolence: true, null, 0f, 0f, ensureNonNumericName: false, forceGenerateNewPawn: true, developmentalStages);
            AdjustPawnSkill(pawn);
            pawns.Add(pawn);
            quest.PawnJoinOffer(pawn, "LetterJoinOfferLabel".Translate(pawn.Named("PAWN")), "LetterJoinOfferTitle".Translate(pawn.Named("PAWN")), "LetterJoinOfferText".Translate(pawn.Named("PAWN"), map.Parent.Named("MAP")), delegate
            {
                quest.JoinPlayer(map.Parent, Gen.YieldSingle(pawn), joinPlayer: true);
                quest.Letter(LetterDefOf.PositiveEvent, null, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, label: "LetterLabelMessageRecruitSuccess".Translate() + ": " + pawn.LabelShortCap, text: "MessageRecruitJoinOfferAccepted".Translate(pawn.Named("RECRUITEE")));
                quest.SignalPass(null, null, lodgerRecruitedSignal);
            }, delegate
            {
                quest.RecordHistoryEvent(HistoryEventDefOf.CharityRefused_ThreatReward_Joiner);
            }, null, null, null, charity: true);
        }
        return pawns;
    }
    private static void AdjustPawnSkill(Pawn pawn)
    {
        SkillRecord intellectual = pawn.skills?.GetSkill(SkillDefOf.Intellectual);
        if (intellectual is not null && intellectual.Level < 8)
        {
            intellectual.Level = IntellectualSkill.RandomInRange;
        }
    }
    private void SetAward(Quest quest)
    {
        QuestPart_Choice questPart_Choice = quest.RewardChoice();
        QuestPart_Choice.Choice choice = new()
        {
            rewards =
            {
                (Reward)new Reward_VisitorsHelp(),
                (Reward)new Reward_PossibleFutureReward()
            }
        };
        if (ModsConfig.IdeologyActive && Faction.OfPlayer.ideos.FluidIdeo is not null)
        {
            choice.rewards.Add(new Reward_DevelopmentPoints(quest));
        }
        questPart_Choice.choices.Add(choice);
    }

    private QuestPart_OARefugeeInteractions WoundedTravelerInteractions(Faction faction, MapParent mapParent) => new()
    {
        allowAssaultColony = true,
        allowBadThought = false,
        allowLeave = false,

        inSignalEnable = QuestGen.slate.Get<string>("inSignal"),
        inSignalDestroyed = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.Destroyed"),
        inSignalSurgeryViolation = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.SurgeryViolation"),
        inSignalKidnapped = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.Kidnapped"),
        inSignalAssaultColony = QuestGen.GenerateNewSignal("AssaultColony"),
        inSignalLeftMap = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.LeftMap"),
        inSignalBanished = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.Banished"),

        outSignalDestroyed_AssaultColony = QuestGen.GenerateNewSignal("LodgerDestroyed_AssaultColony"),
        outSignalArrested_AssaultColony = QuestGen.GenerateNewSignal("LodgerArrested_AssaultColony"),
        outSignalSurgeryViolation_AssaultColony = QuestGen.GenerateNewSignal("LodgerSurgeryViolation_AssaultColony"),
        outSignalPsychicRitualTarget_AssaultColony = QuestGen.GenerateNewSignal("LodgerPsychicRitualTarget_AssaultColony"),

        outSignalLast_Destroyed = QuestGen.GenerateNewSignal("LastLodger_Destroyed"),
        outSignalLast_Arrested = QuestGen.GenerateNewSignal("LastLodger_Arrested"),
        outSignalLast_Kidnapped = QuestGen.GenerateNewSignal("LastLodger_Kidnapped"),
        outSignalLast_Recruited = QuestGen.GenerateNewSignal("LastLodger_Recruited"),
        outSignalLast_LeftMapAllHealthy = QuestGen.GenerateNewSignal("LastLodger_LeftMapAllHealthy"),
        outSignalLast_LeftMapAllNotHealthy = QuestGen.GenerateNewSignal("LastLodger_LeftMapAllNotHealthy"),
        outSignalLast_Banished = QuestGen.GenerateNewSignal("LastLodger_Banished"),

        faction = faction,
        mapParent = mapParent,
        signalListenMode = QuestPart.SignalListenMode.Always
    };

    private void SetQuestEndComp(Quest quest, QuestPart_OARefugeeInteractions questPart_Interactions, List<Pawn> pawns, Faction faction, int lodgerCount)
    {
        quest.Letter(LetterDefOf.NegativeEvent, questPart_Interactions.outSignalDestroyed_AssaultColony, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, "[lodgerDiedAttackPlayerLetterText]", null, "[lodgerDiedAttackPlayerLetterLabel]");
        quest.Letter(LetterDefOf.NegativeEvent, questPart_Interactions.outSignalArrested_AssaultColony, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, "[lodgerArrestedAttackPlayerLetterText]", null, "[lodgerArrestedAttackPlayerLetterLabel]");
        quest.Letter(LetterDefOf.NegativeEvent, questPart_Interactions.outSignalSurgeryViolation_AssaultColony, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, "[lodgerViolatedAttackPlayerLetterText]", null, "[lodgerViolatedAttackPlayerLetterLabel]");
        quest.Letter(LetterDefOf.NegativeEvent, questPart_Interactions.outSignalPsychicRitualTarget_AssaultColony, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, "[lodgerPsychicRitualTargetAttackPlayerLetterText]", null, "[lodgerPsychicRitualTargetAttackPlayerLetterLabel]");

        quest.Letter(LetterDefOf.NegativeEvent, questPart_Interactions.outSignalLast_Destroyed, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, "[lodgersAllDiedLetterText]", null, "[lodgersAllDiedLetterLabel]");
        quest.Letter(LetterDefOf.NegativeEvent, questPart_Interactions.outSignalLast_Arrested, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, "[lodgersAllArrestedLetterText]", null, "[lodgersAllArrestedLetterLabel]");

        quest.AddMemoryThought(pawns, ThoughtDefOf.OtherTravelerDied, questPart_Interactions.outSignalDestroyed_AssaultColony);
        quest.AddMemoryThought(pawns, ThoughtDefOf.OtherTravelerArrested, questPart_Interactions.outSignalArrested_AssaultColony);
        quest.AddMemoryThought(pawns, ThoughtDefOf.OtherTravelerSurgicallyViolated, questPart_Interactions.outSignalSurgeryViolation_AssaultColony);

        quest.End(QuestEndOutcome.Fail, 0, null, questPart_Interactions.outSignalLast_Arrested);
        quest.End(QuestEndOutcome.Fail, 0, null, questPart_Interactions.outSignalLast_Destroyed);

        quest.End(QuestEndOutcome.Fail, 0, null, questPart_Interactions.outSignalDestroyed_AssaultColony, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
        quest.End(QuestEndOutcome.Fail, 0, null, questPart_Interactions.outSignalArrested_AssaultColony, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
        quest.End(QuestEndOutcome.Fail, 0, null, questPart_Interactions.outSignalSurgeryViolation_AssaultColony, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
        quest.End(QuestEndOutcome.Fail, 0, null, questPart_Interactions.outSignalPsychicRitualTarget_AssaultColony, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);

        quest.End(QuestEndOutcome.Fail, 0, null, questPart_Interactions.outSignalLast_Kidnapped, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
        quest.End(QuestEndOutcome.Fail, 0, null, questPart_Interactions.outSignalLast_Banished, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);

        quest.End(QuestEndOutcome.Success, 0, null, questPart_Interactions.outSignalLast_Recruited, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
        quest.End(QuestEndOutcome.Success, 0, null, questPart_Interactions.outSignalLast_LeftMapAllNotHealthy, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);

        quest.SignalPass(delegate
        {
            if (Rand.Chance(0.5f))
            {
                float num2 = (float)(lodgerCount * QuestDurationDays) * 55f;
                FloatRange marketValueRange = new FloatRange(0.7f, 1.3f) * num2 * Find.Storyteller.difficulty.EffectiveQuestRewardValueFactor;
                quest.AddQuestRefugeeDelayedReward(quest.AccepterPawn, faction, pawns, marketValueRange);
            }
            quest.End(QuestEndOutcome.Success, 0, null, null, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
        }, questPart_Interactions.outSignalLast_LeftMapAllHealthy);
    }

}
