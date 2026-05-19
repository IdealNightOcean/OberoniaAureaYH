using OberoniaAurea_Frame;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace OberoniaAurea;

public class WorldObject_MultiPartyTalks : WorldObject_InteractWithFixedCaravanBase
{
    public const float LeaderOffset = 0.05f;
    private static readonly List<(Action, float)> tmpPossibleOutcomes = [];

    public int TotalTopics => participantFactions.Count - 1;
    private int triggeredTopics;
    private int settledTopics;
    private int nextTopicForceTriggeredTick;

    private float baselineNegotiationAbility = 0.75f;
    private string OutcomePropExplanation { get; set; } = string.Empty;

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

    public void SetBaselineNegotiationAbility(float newbaseline)
    {
        baselineNegotiationAbility = newbaseline;
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
                InteractionOutcome outcomeType = MultiPartyTalksUtility.DetermineOutcomeCommon(baselineNegotiationAbility, Rand.Range(0.8f, 1.2f));
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
        StringBuilder sb = new(base.GetInspectString());

        sb.AppendLine("OARK_MultiPartyTalks_Topics".Translate(TotalTopics.Named("TotalTopics"), triggeredTopics.Named("TriggeredTopics"), settledTopics.Named("SettledTopics")));

        if (!string.IsNullOrEmpty(OutcomePropExplanation))
        {
            sb.AppendLine(OutcomePropExplanation);
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
            InteractionOutcome outcomeType = MultiPartyTalksUtility.DetermineOutcomeCommon(usedBaseline, pawnNegotiationAbility);
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

    private void RefreshOutcomePropsExplanation()
    {
        if (associatedFixedCaravan is null)
        {
            OutcomePropExplanation = string.Empty;
            return;
        }

        float pawnNegotiationAbility = OAFrame_PawnUtility.GetMaxStatOfPawns(associatedFixedCaravan.PawnsListForReading, StatDefOf.NegotiationAbility);
        float usedBaseline = baselineNegotiationAbility;
        if (this.Faction.leader is not null && associatedFixedCaravan.PawnsListForReading.Contains(this.Faction.leader))
        {
            usedBaseline *= 0.95f;
        }
        MultiPartyTalksUtility.CalculateOutcomeProbCommon(usedBaseline, pawnNegotiationAbility, out float criticalSuccessProb, out float successProb, out float normalProb, out float failureProb, out float criticalFailureProb);

        OutcomePropExplanation = "OARK_MultiPartyTalks_OutcomePropExplanation".Translate(
            criticalSuccessProb.ToStringPercent("0.##").Named("CriticalSuccessProb"),
            successProb.ToStringPercent("0.##").Named("SuccessProb"),
            normalProb.ToStringPercent("0.##").Named("NormalProb"),
            failureProb.ToStringPercent("0.##").Named("FailureProb"),
            criticalFailureProb.ToStringPercent("0.##").Named("CriticalFailureProb"));

    }

    private void TriggerTopics()
    {
        bool seriousTopic = Rand.Chance(0.3f);
        float topicNegotiationAbility = seriousTopic ? Rand.Range(1.15f, 1.35f) : Rand.Range(0.75f, 1f);
        List<Faction> topicFactions = participantFactions.TakeRandomElements(2).ToList();

        triggeredTopics++;
        nextTopicForceTriggeredTick = Find.TickManager.TicksGame + 30 * 2500;
        throw new NotImplementedException();
    }

    private void ApplyOutcome(InteractionOutcome outcomeType, FixedCaravan fixedCaravan = null)
    {
        MultiPartyTalksRewardData rewardDate = new();
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
                OAFrame_MiscUtility.ColoredIntNamedArgument(rewardDate.OrganizerFactionGoodwill, "Goodwill")));
        }

        if (rewardDate.FactionGoodwill != 0)
        {
            foreach (Faction faction in participantFactions)
            {
                ofPlayer.TryAffectGoodwillWith(
                    other: faction,
                    goodwillChange: rewardDate.FactionGoodwill,
                    canSendMessage: false,
                    canSendHostilityLetter: false,
                    reason: rewardDate.HistoryEvent);
            }
            outcomeBuilder.AppendLine(" ".Translate(
                organizerFaction.Named(KeyLibrary_NamedArgument.FACTION),
                OAFrame_MiscUtility.ColoredIntNamedArgument(rewardDate.FactionGoodwill, "Goodwill")));
        }

        if (rewardDate.AssistPoints != 0)
        {
            OAInteractHandler.Instance.AdjustAssistPoints(rewardDate.AssistPoints);
            outcomeBuilder.AppendLine("OARK_MultiPartyTalks_AssistPoints".Translate(
                organizerFaction.Named(KeyLibrary_NamedArgument.FACTION),
                OAFrame_MiscUtility.ColoredIntNamedArgument(rewardDate.FactionGoodwill, KeyLibrary_NamedArgument.Value)));
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
                outcomeBuilder.AppendLine("OARK_MultiPartyTalks_BecomeHostile".Translate(factinNames.Named("FactinNames")));
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
            outcomeBuilder.AppendLine("OARK_MultiPartyTalks_BecomeNonHostile".Translate(factinNames.Named("FactinNames")));
        }

        Find.LetterStack.ReceiveLetter(
            label: "OARK_MultiPartyTalks_OutcomeLetterLabel".Translate(outcomeLabel.Named("OutcomeLabel")),
            text: outcomeBuilder.ToTaggedString(),
            textLetterDef: rewardDate.LetterDef,
            lookTargets: fixedCaravan,
            relatedFaction: organizerFaction);
    }

    private void SetCommonRewardData(MultiPartyTalksRewardData rewardDate, InteractionOutcome outcomeType)
    {
        switch (outcomeType)
        {
            case InteractionOutcome.CriticalFailure:
                {
                    rewardDate.LetterDef = LetterDefOf.NegativeEvent;
                    rewardDate.HistoryEvent = HistoryEventDefOf.PeaceTalksDisaster;
                    rewardDate.OrganizerFactionGoodwill = -50;
                    rewardDate.FactionGoodwill = -40;
                    break;
                }
            case InteractionOutcome.Failure:
                {
                    rewardDate.LetterDef = LetterDefOf.NegativeEvent;
                    rewardDate.HistoryEvent = HistoryEventDefOf.PeaceTalksBackfire;
                    rewardDate.OrganizerFactionGoodwill = -20;
                    rewardDate.FactionGoodwill = -20;
                    break;
                }
            case InteractionOutcome.Normal:
                {
                    rewardDate.LetterDef = LetterDefOf.NeutralEvent;
                    rewardDate.HistoryEvent = OARK_HistoryEventDefOf.OARK_PeaceTalksNormal;
                    rewardDate.OrganizerFactionGoodwill = 5;
                    rewardDate.FactionGoodwill = 5;
                    break;
                }
            case InteractionOutcome.Success:
                {
                    rewardDate.LetterDef = LetterDefOf.PositiveEvent;
                    rewardDate.HistoryEvent = HistoryEventDefOf.PeaceTalksSuccess;
                    rewardDate.OrganizerFactionGoodwill = 30;
                    rewardDate.FactionGoodwill = 20;
                    rewardDate.AssistPoints = Mathf.CeilToInt(baselineNegotiationAbility / 0.16f);
                    rewardDate.RoyalFavor = Mathf.CeilToInt(baselineNegotiationAbility / 0.4f);
                    break;
                }
            case InteractionOutcome.CriticalSuccess:
                {
                    rewardDate.LetterDef = LetterDefOf.PositiveEvent;
                    rewardDate.HistoryEvent = HistoryEventDefOf.PeaceTalksTriumph;
                    rewardDate.OrganizerFactionGoodwill = 60;
                    rewardDate.FactionGoodwill = 40;
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
