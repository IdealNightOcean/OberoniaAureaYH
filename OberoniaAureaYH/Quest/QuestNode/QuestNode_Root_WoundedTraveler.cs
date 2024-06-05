using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace OberoniaAurea;
public class QuestNode_Root_WoundedTraveler : QuestNode_Root_RefugeeBase
{
    protected override IntRange LodgerCount => new(2, 3);

    private static readonly IntRange InjuryCount = new(2, 3);

    private static readonly int HealthCheckInterval = 30000;
    private static readonly int QuestDurationDays = 7;
    private static readonly int QuestDurationTicks = QuestDurationDays * 60000;
    private static readonly int AssistPoints = 15;

    protected override bool TestRunInt(Slate slate)
    {
        Faction faction = OberoniaAureaYHUtility.OAFaction;
        if (faction == null || faction.HostileTo(Faction.OfPlayer))
        {
            return false;
        }
        return base.TestRunInt(slate);
    }
    protected override void RunInt()
    {
        Faction faction = GetOrGenerateFaction();
        if (faction == null || faction.HostileTo(Faction.OfPlayer))
        {
            return;
        }
        Quest quest = QuestGen.quest;
        Slate slate = QuestGen.slate;
        Map map = QuestGen_Get.GetMap();
        int lodgerCount = LodgerCount.RandomInRange;
        string lodgerRecruitedSignal = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.Recruited");
        string lodgerArrestedSignal = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.Arrested");
        string lodgerBecameMutantSignal = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.BecameMutant");
        string lodgerArrestedOrRecruited = QuestGen.GenerateNewSignal("Lodger_ArrestedOrRecruited");
        quest.AnySignal(new List<string> { lodgerRecruitedSignal, lodgerArrestedSignal }, null, new List<string> { lodgerArrestedOrRecruited });

        List<Pawn> pawns = GeneratePawns(lodgerCount, faction, quest, map, lodgerRecruitedSignal);
        Pawn wounded = pawns[1];
        wounded.health.AddHediff(OA_PawnInfoDefOf.OA_RK_SeriousInjuryII);
        NonLethalDamage(wounded, DamageDefOf.Blunt);
        Pawn asker = pawns.First();


        QuestPart_ExtraFaction questPart_ExtraFaction = quest.ExtraFaction(faction, pawns, ExtraFactionType.MiniFaction, areHelpers: false, [lodgerRecruitedSignal, lodgerBecameMutantSignal]);
        quest.PawnsArrive(pawns, null, map.Parent, null, joinPlayer: true, null, "[lodgersArriveLetterLabel]", "[lodgersArriveLetterText]");
        quest.SetAllApparelLocked(pawns);

        SetAward(quest, faction, lodgerCount, out int goodwillReward);

        QuestPart_OARefugeeInteractions questPart_WoundedTravelerInteractions = WoundedTravelerInteractions(faction, map.Parent);
        questPart_WoundedTravelerInteractions.inSignalArrested = lodgerArrestedSignal;
        questPart_WoundedTravelerInteractions.inSignalRecruited = lodgerRecruitedSignal;
        questPart_WoundedTravelerInteractions.pawns.AddRange(pawns);
        quest.AddPart(questPart_WoundedTravelerInteractions);

        string inSignalAllHealthy = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.AllHealthy");
        QuestPart_AllPawnHealthy questPart_AllPawnHealthy = new()
        {
            inSignalEnable = QuestGen.slate.Get<string>("inSignal"),
            outSignalSuccess = inSignalAllHealthy,
            anyUnhealthyCauseFailure = false,
            allHealthyCauseSuccess = true,
            checkInterval = HealthCheckInterval,
        };
        questPart_AllPawnHealthy.pawns.AddRange(pawns);
        quest.AddPart(questPart_AllPawnHealthy);
        //提前离开
        quest.SignalPassWithFaction(faction, null, delegate
        {
            quest.Letter(LetterDefOf.PositiveEvent, null, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, "[lodgersLeavingEarlyLetterText]", null, "[lodgersLeavingEarlyLetterLabel]");
        }, inSignal: inSignalAllHealthy);
        quest.Leave(pawns, inSignalAllHealthy, sendStandardLetter: false, leaveOnCleanup: false, lodgerArrestedOrRecruited, wakeUp: true);
        //准时离开
        quest.Delay(QuestDurationTicks, delegate
        {
            quest.SignalPassWithFaction(faction, null, delegate
            {
                quest.Letter(LetterDefOf.PositiveEvent, null, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, "[lodgersLeavingLetterText]", null, "[lodgersLeavingLetterLabel]");
            });
            quest.Leave(pawns, null, sendStandardLetter: false, leaveOnCleanup: false, lodgerArrestedOrRecruited, wakeUp: true);
        }, null, inSignalAllHealthy, null, reactivatable: false, null, null, isQuestTimeout: false, "GuestsDepartsIn".Translate(), "GuestsDepartsOn".Translate(), "QuestDelay");

        SetQuestEndComp(quest, questPart_WoundedTravelerInteractions, pawns, goodwillReward, faction, lodgerCount);

        slate.Set("map", map);
        slate.Set("faction", faction);
        slate.Set("questDurationTicks", QuestDurationTicks);
        slate.Set("lodgerCount", lodgerCount);
        slate.Set("lodgersCountMinusOne", lodgerCount - 1);
        slate.Set("lodgers", pawns);
        slate.Set("asker", asker);
    }
    protected override Faction GetOrGenerateFaction()
    {
        return OberoniaAureaYHUtility.OAFaction;
    }

    protected override List<Pawn> GeneratePawns(int lodgerCount, Faction faction, Quest quest, Map map, string lodgerRecruitedSignal = null)
    {
        List<Pawn> pawns = [];
        for (int i = 0; i < lodgerCount; i++)
        {
            DevelopmentalStage developmentalStages = DevelopmentalStage.Adult;
            Pawn pawn = quest.GeneratePawn(OA_PawnGenerateDefOf.OA_RK_Traveller, faction, allowAddictions: false, null, 0f, mustBeCapableOfViolence: true, null, 0f, 0f, ensureNonNumericName: false, forceGenerateNewPawn: true, developmentalStages);
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
    private void SetAward(Quest quest, Faction faction, int lodgerCount, out int goodwillReward)
    {
        Reward_Goodwill reward_Goodwill = new()
        {
            faction = faction,
            amount = lodgerCount + 8
        };
        goodwillReward = reward_Goodwill.amount;
        QuestPart_Choice questPart_Choice = quest.RewardChoice();
        QuestPart_Choice.Choice choice = new()
        {
            rewards =
            {
                (Reward)new Reward_VisitorsHelp(),
                (Reward)reward_Goodwill
            }
        };
        if (ModsConfig.RoyaltyActive)
        {
            choice.rewards.Add(new Reward_PossibleFutureReward());
        }
        if (ModsConfig.IdeologyActive && Faction.OfPlayer.ideos.FluidIdeo != null)
        {
            choice.rewards.Add(new Reward_DevelopmentPoints(quest));
        }
        questPart_Choice.choices.Add(choice);
    }

    private QuestPart_OARefugeeInteractions WoundedTravelerInteractions(Faction faction, MapParent mapParent) => new()
    {
        allowAssaultColony = false,
        allowLeave = true,
        allowBadThought = false,

        inSignalEnable = QuestGen.slate.Get<string>("inSignal"),
        inSignalDestroyed = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.Destroyed"),
        inSignalSurgeryViolation = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.SurgeryViolation"),
        inSignalKidnapped = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.Kidnapped"),
        inSignalAssaultColony = QuestGen.GenerateNewSignal("AssaultColony"),
        inSignalLeftMap = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.LeftMap"),
        inSignalBanished = QuestGenUtility.HardcodedSignalWithQuestID("lodgers.Banished"),

        outSignalDestroyed_LeaveColony = QuestGen.GenerateNewSignal("LodgerDestroyed_LeaveColony"),
        outSignalArrested_LeaveColony = QuestGen.GenerateNewSignal("LodgerArrested_LeaveColony"),
        outSignalSurgeryViolation_LeaveColony = QuestGen.GenerateNewSignal("LodgerSurgeryViolation_LeaveColony"),
        outSignalPsychicRitualTarget_LeaveColony = QuestGen.GenerateNewSignal("LodgerPsychicRitualTarget_LeaveColony"),

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

    private void SetQuestEndComp(Quest quest, QuestPart_OARefugeeInteractions questPart_Interactions, List<Pawn> pawns, int goodwillReward, Faction faction, int lodgerCount)
    {
        quest.Letter(LetterDefOf.NegativeEvent, questPart_Interactions.outSignalDestroyed_LeaveColony, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, "[lodgerDiedLeaveMapLetterText]", null, "[lodgerDiedLeaveMapLetterLabel]");
        quest.Letter(LetterDefOf.NegativeEvent, questPart_Interactions.outSignalArrested_LeaveColony, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, "[lodgerArrestedLeaveMapLetterText]", null, "[lodgerArrestedLeaveMapLetterLabel]");
        quest.Letter(LetterDefOf.NegativeEvent, questPart_Interactions.outSignalSurgeryViolation_LeaveColony, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, "[lodgerViolatedLeaveMapLetterText]", null, "[lodgerViolatedLeaveMapLetterLabel]");
        quest.Letter(LetterDefOf.NegativeEvent, questPart_Interactions.outSignalPsychicRitualTarget_LeaveColony, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, "[lodgerPsychicRitualTargetLeaveMapLetterText]", null, "[lodgerPsychicRitualTargetLeaveMapLetterLabel]");

        quest.Letter(LetterDefOf.NegativeEvent, questPart_Interactions.outSignalLast_Destroyed, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, "[lodgersAllDiedLetterText]", null, "[lodgersAllDiedLetterLabel]");
        quest.Letter(LetterDefOf.NegativeEvent, questPart_Interactions.outSignalLast_Arrested, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, "[lodgersAllArrestedLetterText]", null, "[lodgersAllArrestedLetterLabel]");

        quest.AddMemoryThought(pawns, ThoughtDefOf.OtherTravelerDied, questPart_Interactions.outSignalDestroyed_LeaveColony);
        quest.AddMemoryThought(pawns, ThoughtDefOf.OtherTravelerArrested, questPart_Interactions.outSignalArrested_LeaveColony);
        quest.AddMemoryThought(pawns, ThoughtDefOf.OtherTravelerSurgicallyViolated, questPart_Interactions.outSignalSurgeryViolation_LeaveColony);

        quest.End(QuestEndOutcome.Fail, -goodwillReward, faction, questPart_Interactions.outSignalLast_Destroyed);
        quest.End(QuestEndOutcome.Fail, -goodwillReward, faction, questPart_Interactions.outSignalLast_Arrested);

        quest.End(QuestEndOutcome.Fail, -goodwillReward, faction, questPart_Interactions.outSignalDestroyed_LeaveColony, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
        quest.End(QuestEndOutcome.Fail, -goodwillReward, faction, questPart_Interactions.outSignalArrested_LeaveColony, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
        quest.End(QuestEndOutcome.Fail, -goodwillReward, faction, questPart_Interactions.outSignalSurgeryViolation_LeaveColony, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
        quest.End(QuestEndOutcome.Fail, -goodwillReward, null, questPart_Interactions.outSignalPsychicRitualTarget_LeaveColony, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);

        quest.End(QuestEndOutcome.Fail, -goodwillReward, faction, questPart_Interactions.outSignalLast_Kidnapped, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
        quest.End(QuestEndOutcome.Fail, -goodwillReward, faction, questPart_Interactions.outSignalLast_Banished, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);

        quest.AddPart(new QuestPart_OaAssistPointsChange(questPart_Interactions.outSignalLast_Recruited, AssistPoints));
        quest.End(QuestEndOutcome.Success, 0, null, questPart_Interactions.outSignalLast_Recruited, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);

        quest.AddPart(new QuestPart_OaAssistPointsChange(questPart_Interactions.outSignalLast_LeftMapAllNotHealthy, AssistPoints));
        quest.End(QuestEndOutcome.Success, goodwillReward / 2, faction, questPart_Interactions.outSignalLast_LeftMapAllNotHealthy, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);

        quest.AddPart(new QuestPart_OaAssistPointsChange(questPart_Interactions.outSignalLast_LeftMapAllHealthy, AssistPoints));
        quest.SignalPass(delegate
        {
            if (ModsConfig.RoyaltyActive)
            {
                float num3 = (float)(lodgerCount * QuestDurationDays) * 0.55f;
                FloatRange marketValueRange = new FloatRange(0.7f, 1.3f) * num3 * Find.Storyteller.difficulty.EffectiveQuestRewardValueFactor;
                quest.AddQuestRefugeeDelayedReward(quest.AccepterPawn, faction, pawns, marketValueRange);
            }
            quest.End(QuestEndOutcome.Success, goodwillReward, faction, null, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
        }, questPart_Interactions.outSignalLast_LeftMapAllHealthy);
    }

    protected static void NonLethalDamage(Pawn p, DamageDef fixedDamageDef = null) //造成不致命、不残疾的伤害
    {
        if (p.Downed)
        {
            return;
        }
        p.health.forceDowned = true;
        IEnumerable<BodyPartRecord> source = HittablePartsViolence(p);
        int num = 0;
        while (num < InjuryCount.RandomInRange)
        {
            num++;
            if (!source.Any())
            {
                break;
            }
            BodyPartRecord bodyPartRecord = source.RandomElement();
            float maxHealth = bodyPartRecord.def.GetMaxHealth(p);
            float partHealth = p.health.hediffSet.GetPartHealth(bodyPartRecord);
            int min = Mathf.Min(Mathf.RoundToInt(maxHealth * 0.3f), (int)partHealth - 1);
            int max = Mathf.Min(Mathf.RoundToInt(maxHealth * 0.8f), (int)partHealth - 1);
            int num2 = Rand.RangeInclusive(min, max);
            DamageDef damageDef = fixedDamageDef ?? HealthUtility.RandomViolenceDamageType();
            HediffDef hediffDefFromDamage = HealthUtility.GetHediffDefFromDamage(damageDef, p, bodyPartRecord);
            if (p.health.WouldDieAfterAddingHediff(hediffDefFromDamage, bodyPartRecord, num2))
            {
                break;
            }
            DamageInfo dinfo = new(damageDef, num2, 999f, -1f, null, bodyPartRecord);
            dinfo.SetAllowDamagePropagation(val: false);
            p.TakeDamage(dinfo);
        }
        if (p.Dead)
        {
            StringBuilder stringBuilder = new();
            stringBuilder.AppendLine(string.Concat(p, " died during GiveInjuriesToForceDowned"));
            for (int i = 0; i < p.health.hediffSet.hediffs.Count; i++)
            {
                stringBuilder.AppendLine("   -" + p.health.hediffSet.hediffs[i].ToString());
            }
            Log.Error(stringBuilder.ToString());
        }
        p.health.forceDowned = false;
    }
    protected static IEnumerable<BodyPartRecord> HittablePartsViolence(Pawn pawn)
    {
        HediffSet hediffSet = pawn.health.hediffSet;
        return from x in hediffSet.GetNotMissingParts()
               where x.depth == BodyPartDepth.Outside || (x.depth == BodyPartDepth.Inside && x.def.IsSolid(x, hediffSet.hediffs))
               where !pawn.health.hediffSet.hediffs.Any((Hediff y) => y.Part == x && y.CurStage != null && y.CurStage.partEfficiencyOffset < 0f)
               select x;
    }

}
