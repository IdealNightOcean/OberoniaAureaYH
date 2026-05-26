using OberoniaAurea_Frame;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace OberoniaAurea;

public class WorldObject_MultiPartyTalks : WorldObject_InteractWithFixedCaravanBase
{
    public int TotalTopics => participantFactions.Count - 1;
    private int triggeredTopics;
    public int TriggeredTopics => triggeredTopics;
    private int settledTopics;
    private int nextTopicForceTriggeredTick;

    private float baselineNegotiationAbility = 0.75f;
    private float testPawnNegotiationAbility = 1f;

    public float BaselineNegotiationAbility => baselineNegotiationAbility;
    private string OutcomePropExplanation { get; set; } = string.Empty;

    public override int TicksNeeded => 2500;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref triggeredTopics, nameof(triggeredTopics), 0);
        Scribe_Values.Look(ref settledTopics, nameof(settledTopics), 0);
        Scribe_Values.Look(ref baselineNegotiationAbility, nameof(baselineNegotiationAbility), 0.75f);
        Scribe_Values.Look(ref nextTopicForceTriggeredTick, nameof(nextTopicForceTriggeredTick), 0);
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            RefreshOutcomePropsExplanation();
        }
    }

    public void AdjustBaselineNegotiationAbility(float change)
    {
        baselineNegotiationAbility += change;
        RefreshOutcomePropsExplanation();
    }
    public void SetBaselineNegotiationAbility(float newbaseline)
    {
        baselineNegotiationAbility = baselineNegotiationAbility = Mathf.Max(0f, newbaseline);
        RefreshOutcomePropsExplanation();
    }

    public void OnTopicSettled()
    {
        settledTopics++;
        if (settledTopics >= TotalTopics)
        {
            if (isWorking)
            {
                EndWork();
            }
            else
            {
                InteractionOutcome outcomeType = DiplomacyUtility.DetermineOutcomeCommon(baselineNegotiationAbility, Rand.Range(0.8f, 1.2f));
                ApplyOutcome(outcomeType);
                QuestUtility.SendQuestTargetSignals(questTags, "Resolved", this.Named(KeyLibrary_NamedArgument.SUBJECT));
            }
        }
    }

    public override void PostAdd()
    {
        base.PostAdd();
        nextTopicForceTriggeredTick = Find.TickManager.TicksGame + 30 * 2500;
        participantFactions.Add(this.Faction);
        float initBaseline = MultiPartyTalksUtility.CalculateBaselineNegotiationAbility(participantFactions, resultOnly: true, out _);
        SetBaselineNegotiationAbility(initBaseline);
    }

    public override string GetInspectString()
    {
        StringBuilder sb = new(64);
        sb.AppendLine(base.GetInspectString());
        sb.Append("OARK_MultiPartyTalks_Topics".Translate(TotalTopics.Named("TotalTopics"), triggeredTopics.Named("TriggeredTopics"), settledTopics.Named("SettledTopics")));

        if (OAFrame_MiscUtility.GodMode)
        {
            sb.AppendLine();
            sb.AppendLine("————————");
            sb.AppendLine("DEV: ");
            sb.AppendLine($"谈判能力基准需求：{baselineNegotiationAbility.ToStringPercent()}");
            sb.AppendLine($"测试谈判能力：{baselineNegotiationAbility.ToStringPercent()}");
            if (!string.IsNullOrEmpty(OutcomePropExplanation))
            {
                sb.Append(OutcomePropExplanation);
            }
        }
        return sb.ToString();
    }

    public override bool StartWork(Caravan caravan)
    {
        Pawn pawn = BestCaravanPawnUtility.FindBestDiplomat(caravan);
        if (pawn is null)
        {
            Messages.Message("OAFrame_MessageNoDiplomat".Translate(), caravan, MessageTypeDefOf.NegativeEvent, historical: false);
            return false;
        }

        if (base.StartWork(caravan))
        {
            RefreshOutcomePropsExplanation();
            return true;
        }

        return false;
    }

    protected override void WorkTickInterval(int delta)
    {
        if ((ticksRemaining -= delta) > 0)
            return;

        if (settledTopics >= TotalTopics)
        {
            EndWork();
            return;
        }

        ticksRemaining = 2500;
        if (triggeredTopics < TotalTopics && (Find.TickManager.TicksGame > nextTopicForceTriggeredTick || Rand.Chance(0.04f)))
        {
            TriggerTopics();
        }
    }

    protected override void InterruptWork() { }

    protected override void FinishWork()
    {
        try
        {
            float pawnNegotiationAbility = associatedFixedCaravan is null ? Rand.Range(0.8f, 1.2f) : OAFrame_PawnUtility.GetMaxStatOfPawns(associatedFixedCaravan.PawnsListForReading, StatDefOf.NegotiationAbility);
            float usedBaseline = baselineNegotiationAbility;
            if (this.Faction.leader is not null && associatedFixedCaravan is not null && associatedFixedCaravan.PawnsListForReading.Contains(this.Faction.leader))
            {
                usedBaseline *= 0.95f;
            }
            InteractionOutcome outcomeType = DiplomacyUtility.DetermineOutcomeCommon(usedBaseline, pawnNegotiationAbility);
            ApplyOutcome(outcomeType, associatedFixedCaravan);
        }
        finally
        {
            QuestUtility.SendQuestTargetSignals(questTags, "Resolved", this.Named(KeyLibrary_NamedArgument.SUBJECT));
        }
    }

    public override void PostConvertToCaravan(Caravan caravan)
    {
        base.PostConvertToCaravan(caravan);
        RefreshOutcomePropsExplanation();
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (Gizmo gizmo in base.GetGizmos())
            yield return gizmo;

        if (!DebugSettings.ShowDevGizmos)
            yield break;

        yield return new Command_Action
        {
            defaultLabel = "baseline +10%",
            action = () =>
            {
                baselineNegotiationAbility += 0.1f;
                RefreshOutcomePropsExplanation();
            }
        };
        yield return new Command_Action
        {
            defaultLabel = "baseline -10%",
            action = () =>
            {
                baselineNegotiationAbility -= 0.1f;
                RefreshOutcomePropsExplanation();
            }
        };
        yield return new Command_Action
        {
            defaultLabel = "testAbility +10%",
            action = () =>
            {
                testPawnNegotiationAbility += 0.1f;
                RefreshOutcomePropsExplanation();
            }
        };
        yield return new Command_Action
        {
            defaultLabel = "testAbility -10%",
            action = () =>
            {
                testPawnNegotiationAbility -= 0.1f;
                RefreshOutcomePropsExplanation();
            }
        };
        yield return new Command_Action
        {
            defaultLabel = "trigger topic",
            action = TriggerTopics
        };
        yield return new Command_Action
        {
            defaultLabel = "settle topic",
            action = OnTopicSettled
        };
    }

    private void RefreshOutcomePropsExplanation()
    {
        StringBuilder textBuilder = new(64);
        float usedBaseline = baselineNegotiationAbility;

        textBuilder.AppendLine($"测试概率分布：根据 {nameof(testPawnNegotiationAbility)} 计算");
        textBuilder.Append(DiplomacyUtility.OutcomeProbabilityInfo(usedBaseline, testPawnNegotiationAbility));

        if (associatedFixedCaravan is not null)
        {
            float pawnNegotiationAbility = OAFrame_PawnUtility.GetMaxStatOfPawns(associatedFixedCaravan.PawnsListForReading, StatDefOf.NegotiationAbility);
            if (this.Faction.leader is not null && associatedFixedCaravan.PawnsListForReading.Contains(this.Faction.leader))
            {
                usedBaseline *= 0.95f;
            }

            textBuilder.AppendLine();
            textBuilder.AppendLine("远行队实际概率分布：");
            textBuilder.Append(DiplomacyUtility.OutcomeProbabilityInfo(usedBaseline, testPawnNegotiationAbility));
        }

        OutcomePropExplanation = textBuilder.ToString();
    }

    private void TriggerTopics()
    {
        triggeredTopics++;
        nextTopicForceTriggeredTick = Find.TickManager.TicksGame + 30 * 2500;

        bool seriousTopic = Rand.Chance(0.3f);
        float topicNegotiationAbility = seriousTopic ? Rand.Range(1.15f, 1.35f) : Rand.Range(0.75f, 1f);
        List<Faction> topicFactions = participantFactions.TakeRandomElements(2).ToList();

        ChoiceLetter_MultiPartyTalksTopic letter = (ChoiceLetter_MultiPartyTalksTopic)LetterMaker.MakeLetter(
            label: "OARK_MultiPartyTalks_NewTopic".Translate(),
            text: string.Empty,
            def: OARK_LetterDefOf.OARK_MultiPartyTalksTopic,
            relatedFaction: this.Faction,
            quest: this.AssociatedQuest);
        letter.InitMultiPartyTalksLetter(this, topicNegotiationAbility, seriousTopic, topicFactions);
        letter.StartTimeout(3 * 2500);
        Find.LetterStack.ReceiveLetter(letter);
    }

    private void ApplyOutcome(InteractionOutcome outcomeType, FixedCaravan fixedCaravan = null)
    {
        DiplomacyRewardData rewardDate = new();
        SetCommonRewardData(rewardDate, outcomeType);

        StringBuilder outcomeBuilder = new(128);
        TaggedString outcomeLabel = $"OARK_InteractionOutcomeLabel_{outcomeType}".Translate();
        outcomeBuilder.AppendLine("OARK_MultiPartyTalks_OutcomeLetterText".Translate(outcomeLabel.Named("OutcomeLabel")));
        Faction ofPlayer = Faction.OfPlayer;
        Faction organizerFaction = this.Faction;

        if (rewardDate.OrganizerFactionGoodwill != 0 && organizerFaction is not null)
        {
            ofPlayer.TryAffectGoodwillWith(
                other: organizerFaction,
                goodwillChange: rewardDate.OrganizerFactionGoodwill,
                canSendMessage: false,
                canSendHostilityLetter: false,
                reason: rewardDate.HistoryEvent);

            outcomeBuilder.AppendLine("OARK_MultiPartyTalks_OrganizerFactionGoodwill".Translate(
                organizerFaction.Named(KeyLibrary_NamedArgument.FACTION),
                OAFrame_TextUtility.ColoredIntNamedArgument(rewardDate.OrganizerFactionGoodwill, KeyLibrary_NamedArgument.Goodwill)));
        }

        if (rewardDate.OtherFactionGoodwill != 0)
        {
            foreach (Faction faction in participantFactions)
            {
                ofPlayer.TryAffectGoodwillWith(
                    other: faction,
                    goodwillChange: rewardDate.OtherFactionGoodwill,
                    canSendMessage: false,
                    canSendHostilityLetter: false,
                    reason: rewardDate.HistoryEvent);
            }
            outcomeBuilder.AppendLine(" ".Translate(
                organizerFaction.Named(KeyLibrary_NamedArgument.FACTION),
                OAFrame_TextUtility.ColoredIntNamedArgument(rewardDate.OtherFactionGoodwill, KeyLibrary_NamedArgument.Goodwill)));
        }

        if (rewardDate.AssistPoints != 0)
        {
            OAInteractHandler.Instance.AdjustAssistPoints(rewardDate.AssistPoints);
            outcomeBuilder.AppendLine("OARK_MultiPartyTalks_AssistPoints".Translate(
                organizerFaction.Named(KeyLibrary_NamedArgument.FACTION),
                OAFrame_TextUtility.ColoredIntNamedArgument(rewardDate.OtherFactionGoodwill, KeyLibrary_NamedArgument.Value)));
        }

        Pawn negotiator = null;
        if (fixedCaravan is not null)
        {
            (negotiator, _) = OAFrame_PawnUtility.GetMaxStatPawn(fixedCaravan.PawnsListForReading, StatDefOf.NegotiationAbility);

            if (!rewardDate.Gifts.NullOrEmpty())
            {
                outcomeBuilder.AppendLine("OARK_MultiPartyTalks_Gifts".Translate(GenLabel.ThingsLabel(rewardDate.Gifts).Named("Gifts")));
                OAFrame_FixedCaravanUtility.GiveThings(fixedCaravan, rewardDate.Gifts);
            }
        }
        if (negotiator is not null)
        {
            if (rewardDate.RoyalFavor != 0 && organizerFaction.def.HasRoyalTitles)
            {
                negotiator.royalty.GainFavor(organizerFaction, rewardDate.RoyalFavor);
                outcomeBuilder.AppendLine("PeaceTalksRoyalFavorGain".Translate(negotiator.LabelShort, rewardDate.RoyalFavor.ToString(), organizerFaction.Named(KeyLibrary_NamedArgument.FACTION), negotiator.Named(KeyLibrary_NamedArgument.PAWN)));
            }

            if (negotiator.skills is not null)
            {
                negotiator.skills.Learn(SkillDefOf.Social, 6000f);
                outcomeBuilder.AppendLine("PeaceTalksSocialXPGain".Translate(negotiator.LabelShort, 6000f.ToString("F0"), negotiator.Named(KeyLibrary_NamedArgument.PAWN)));
            }
        }

        if (outcomeType == InteractionOutcome.CriticalFailure)
        {
            List<Faction> targetFactions = participantFactions.Where(f => !f.HostileTo(ofPlayer) && !f.def.permanentEnemy).TakeRandomElements(2).ToList();
            if (!targetFactions.NullOrEmpty())
            {
                foreach (Faction f in targetFactions)
                {
                    int goodwillToMakeHostile = ofPlayer.GoodwillToMakeHostile(f) - 10;
                    ofPlayer.TryAffectGoodwillWith(
                        other: f,
                        goodwillChange: goodwillToMakeHostile,
                        canSendMessage: false,
                        canSendHostilityLetter: false,
                        reason: rewardDate.HistoryEvent);
                }
                string factinNames = string.Join(", ", targetFactions.Select(f => f.NameColored));
                outcomeBuilder.AppendLine("OARK_MultiPartyTalks_BecomeHostile".Translate(factinNames.Named(KeyLibrary_NamedArgument.FactionNames)));
            }
        }
        if (outcomeType == InteractionOutcome.CriticalSuccess)
        {
            List<Faction> targetFactions = participantFactions.Where(f => f.HostileTo(ofPlayer) && !f.def.permanentEnemy).TakeRandomElements(2).ToList();
            if (!targetFactions.NullOrEmpty())
            {
                foreach (Faction f in targetFactions)
                {
                    int goodwillToMakeNonHostile = Mathf.Max(0, -ofPlayer.GoodwillWith(f)) + 15;
                    ofPlayer.TryAffectGoodwillWith(
                        other: f,
                        goodwillChange: goodwillToMakeNonHostile,
                        canSendMessage: false,
                        canSendHostilityLetter: false,
                        reason: rewardDate.HistoryEvent);
                }
            }
            string factinNames = string.Join(", ", targetFactions.Select(f => f.NameColored));
            outcomeBuilder.AppendLine("OARK_MultiPartyTalks_BecomeNonHostile".Translate(factinNames.Named(KeyLibrary_NamedArgument.FactionNames)));
        }

        Find.LetterStack.ReceiveLetter(
            label: "OARK_MultiPartyTalks_OutcomeLetterLabel".Translate(outcomeLabel.Named("OutcomeLabel")),
            text: outcomeBuilder.ToTaggedString(),
            textLetterDef: rewardDate.LetterDef,
            lookTargets: fixedCaravan,
            relatedFaction: organizerFaction);
    }

    private void SetCommonRewardData(DiplomacyRewardData rewardDate, InteractionOutcome outcomeType)
    {
        switch (outcomeType)
        {
            case InteractionOutcome.CriticalFailure:
                {
                    rewardDate.LetterDef = LetterDefOf.NegativeEvent;
                    rewardDate.HistoryEvent = HistoryEventDefOf.PeaceTalksDisaster;
                    rewardDate.OrganizerFactionGoodwill = -50;
                    rewardDate.OtherFactionGoodwill = -40;
                    break;
                }
            case InteractionOutcome.Failure:
                {
                    rewardDate.LetterDef = LetterDefOf.NegativeEvent;
                    rewardDate.HistoryEvent = HistoryEventDefOf.PeaceTalksBackfire;
                    rewardDate.OrganizerFactionGoodwill = -20;
                    rewardDate.OtherFactionGoodwill = -20;
                    break;
                }
            case InteractionOutcome.Normal:
                {
                    rewardDate.LetterDef = LetterDefOf.NeutralEvent;
                    rewardDate.HistoryEvent = OARK_HistoryEventDefOf.OARK_PeaceTalksNormal;
                    rewardDate.OrganizerFactionGoodwill = 5;
                    rewardDate.OtherFactionGoodwill = 5;
                    break;
                }
            case InteractionOutcome.Success:
                {
                    rewardDate.LetterDef = LetterDefOf.PositiveEvent;
                    rewardDate.HistoryEvent = HistoryEventDefOf.PeaceTalksSuccess;
                    rewardDate.OrganizerFactionGoodwill = 30;
                    rewardDate.OtherFactionGoodwill = 20;
                    rewardDate.AssistPoints = Mathf.CeilToInt(baselineNegotiationAbility / 0.16f);
                    rewardDate.RoyalFavor = Mathf.CeilToInt(baselineNegotiationAbility / 0.4f);
                    break;
                }
            case InteractionOutcome.CriticalSuccess:
                {
                    rewardDate.LetterDef = LetterDefOf.PositiveEvent;
                    rewardDate.HistoryEvent = HistoryEventDefOf.PeaceTalksTriumph;
                    rewardDate.OrganizerFactionGoodwill = 60;
                    rewardDate.OtherFactionGoodwill = 40;
                    rewardDate.AssistPoints = Mathf.CeilToInt(baselineNegotiationAbility / 0.08f);
                    rewardDate.RoyalFavor = Mathf.CeilToInt(baselineNegotiationAbility / 0.2f);

                    ThingSetMakerParams parms = default;
                    parms.makingFaction = Faction;
                    parms.techLevel = Faction.def.techLevel;
                    parms.maxTotalMass = 20f;
                    parms.totalMarketValueRange = new FloatRange(500f, 1200f);
                    parms.tile = Tile;
                    rewardDate.Gifts.AddRange(ThingSetMakerDefOf.Reward_ItemsStandard.root.Generate(parms));
                    break;
                }
            default:
                {
                    rewardDate.LetterDef = LetterDefOf.NeutralEvent;
                    rewardDate.HistoryEvent = OARK_HistoryEventDefOf.OARK_PeaceTalksNormal;
                    break;
                }
        }
    }

}
