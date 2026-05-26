using OberoniaAurea_Frame;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Grammar;

namespace OberoniaAurea;

public class ChoiceLetter_MultiPartyTalksTopic : ChoiceLetter
{
    public WorldObject_MultiPartyTalks talksSite;
    private float topicNegotiationAbility = 1f;
    private bool isSeriousTopic;
    private List<Faction> topicFactions;

    public override bool CanDismissWithRightClick => false;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref talksSite, nameof(talksSite));
        Scribe_Values.Look(ref topicNegotiationAbility, nameof(topicNegotiationAbility), 1f);
        Scribe_Values.Look(ref isSeriousTopic, nameof(isSeriousTopic), defaultValue: false);
        Scribe_Collections.Look(ref topicFactions, nameof(topicFactions), LookMode.Reference);
    }

    public void InitMultiPartyTalksLetter(WorldObject_MultiPartyTalks talksSite, float topicNegotiationAbility, bool isSeriousTopic, IEnumerable<Faction> factions)
    {
        this.talksSite = talksSite;
        this.topicNegotiationAbility = topicNegotiationAbility;
        this.isSeriousTopic = isSeriousTopic;
        topicFactions ??= [];
        topicFactions.AddRange(factions);
        this.Text = TopicInfoText();
    }

    public override IEnumerable<DiaOption> Choices
    {
        get
        {
            if (talksSite is null || !talksSite.IsWorking || talksSite.Destroyed)
            {
                yield return Option_Close;
                yield break;
            }

            yield return Option_DeepStatement;
            yield return Option_ShallowStatement;
            yield return Option_Ingore;

            yield return Option_Postpone;
        }
    }

    private DiaOption Option_DeepStatement => new("OARK_MultiPartyTalksTopic_DeepStatement".Translate())
    {
        action = delegate
        {
            try
            {
                Statement(isDeepStatement: true);
            }
            catch (Exception ex)
            {
                Log.Error($"[OARK] 在多方会谈议题标题出现异常：\n{ex}");
            }

            talksSite?.OnTopicSettled();
            Find.LetterStack.RemoveLetter(this);
        },
        resolveTree = true
    };

    private DiaOption Option_ShallowStatement => new("OARK_MultiPartyTalksTopic_ShallowStatement".Translate())
    {
        action = delegate
        {
            try
            {
                Statement(isDeepStatement: false);
            }
            catch (Exception ex)
            {
                Log.Error($"[OARK] 在多方会谈议题标题出现异常：\n{ex}");
            }
            talksSite?.OnTopicSettled();
            Find.LetterStack.RemoveLetter(this);
        },
        resolveTree = true
    };

    private DiaOption Option_Ingore => new("OARK_MultiPartyTalksTopic_Ingore".Translate())
    {
        action = delegate
        {
            try
            {
                Ingore();
            }
            catch (Exception ex)
            {
                Log.Error($"[OARK] 在多方会谈议题标题出现异常：\n{ex}");
            }

            talksSite?.OnTopicSettled();
            Find.LetterStack.RemoveLetter(this);
        },
        resolveTree = true
    };

    public override void Removed()
    {
        base.Removed();
        talksSite = null;
        topicFactions = null;
    }

    private bool IsSuccessfulTopic()
    {
        float usedBaseline = topicNegotiationAbility;
        float pawnNegotiationAbility = OAFrame_PawnUtility.GetMaxStatOfPawns(talksSite.AssociatedFixedCaravan.PawnsListForReading, StatDefOf.NegotiationAbility);
        if (talksSite.Faction.leader is not null && talksSite.AssociatedFixedCaravan.PawnsListForReading.Contains(talksSite.Faction.leader))
        {
            usedBaseline *= 0.95f;
        }
        InteractionOutcome outcome = DiplomacyUtility.DetermineOutcomeCommon(usedBaseline, pawnNegotiationAbility);
        return outcome >= InteractionOutcome.Normal;
    }

    private void Ingore()
    {
        StringBuilder textBuilder = new(256);
        if (talksSite?.AssociatedFixedCaravan is not null)
        {
            (Pawn negotiator, _) = OAFrame_PawnUtility.GetMaxStatPawn(talksSite.AssociatedFixedCaravan.PawnsListForReading, StatDefOf.NegotiationAbility);

            if (negotiator?.needs?.mood is not null)
            {
                negotiator.needs.mood.thoughts.memories.TryGainMemory(OARK_ThoughtDefOf.OARK_TravelAtPublicCost);
                textBuilder.AppendLine("OARK_MultiPartyTalksTopic_IngoreOutcome".Translate(negotiator.Named(KeyLibrary_NamedArgument.PAWN)));
            }
        }

        Dialog_NodeTreeWithFactionInfo confirmNodeTree = OAFrame_DiaUtility.DefaultConfirmDiaNodeTreeWithFactionInfo(textBuilder.ToString(), talksSite?.Faction);
        Find.WindowStack.Add(confirmNodeTree);

        textBuilder.Insert(0, "\n");
        textBuilder.Insert(0, this.Text);
        this.Text = textBuilder.ToTaggedString();

        if (Rand.Chance(0.6f))
        {
            ExtraTopicEvent(isDeepStatement: false, isSuccessful: false, easterEggAvailable: true);
        }
    }

    private void Statement(bool isDeepStatement)
    {
        TopicRewardData rewardData = new();
        bool isSuccessful = IsSuccessfulTopic();
        if (isDeepStatement)
        {
            rewardData.NegotiatorXP = 2000;
            rewardData.IntelligenceDraftCount = 25 + Mathf.Min(75, Mathf.CeilToInt(talksSite.BaselineNegotiationAbility / 0.1f) * (isSeriousTopic ? 2 : 1));

            if (isSuccessful)
            {
                rewardData.BaselineAdjust = -Rand.Range(0.05f, 0.1f);
                rewardData.AssintPoints = isSeriousTopic ? 10 : 5;
                rewardData.RoyalFavor = isSeriousTopic ? 5 : 2;

                rewardData.OrganizerGoodwill = 10;
                rewardData.TopicFactionsGoodwill = 10;
                rewardData.OtherFactionsGoodwill = Rand.Bool ? 0 : 5;
            }
            else
            {
                rewardData.BaselineAdjust = 0.05f;
                rewardData.OrganizerGoodwill = -10;
                rewardData.TopicFactionsGoodwill = -10;
                rewardData.OtherFactionsGoodwill = Rand.Bool ? 0 : -5;
            }
        }
        else
        {
            rewardData.NegotiatorXP = 1000;
            rewardData.IntelligenceDraftCount = 10 + Mathf.Min(75, Mathf.CeilToInt(talksSite.BaselineNegotiationAbility / 0.15f) * (isSeriousTopic ? 2 : 1));

            if (isSuccessful)
            {
                rewardData.BaselineAdjust = -0.05f;

                rewardData.OrganizerGoodwill = 5;
                rewardData.TopicFactionsGoodwill = 5;
            }
            else
            {
                rewardData.TopicFactionsGoodwill = -5;
            }
        }

        ApplyCommonReward(rewardData);

        StringBuilder textBuilder = new(256);
        textBuilder.AppendLine(RewardDataText(rewardData));

        Dialog_NodeTreeWithFactionInfo confirmNodeTree = OAFrame_DiaUtility.DefaultConfirmDiaNodeTreeWithFactionInfo(textBuilder.ToString(), talksSite?.Faction);
        Find.WindowStack.Add(confirmNodeTree);

        textBuilder.Insert(0, "\n");
        textBuilder.Insert(0, this.Text);
        this.Text = textBuilder.ToTaggedString();

        if (Rand.Chance(0.4f))
        {
            ExtraTopicEvent(isDeepStatement, isSuccessful, easterEggAvailable: false);
        }
    }

    private void ExtraTopicEvent(bool isDeepStatement, bool isSuccessful, bool easterEggAvailable)
    {
        MultiPartyTalksTopicEventDef.EventType eventType = MultiPartyTalksTopicEventDef.EventType.None;
        eventType |= (isDeepStatement ? MultiPartyTalksTopicEventDef.EventType.Serious : MultiPartyTalksTopicEventDef.EventType.Normal);
        if (easterEggAvailable)
            eventType |= MultiPartyTalksTopicEventDef.EventType.EasterEgg;

        (Pawn negotiator, _) = OAFrame_PawnUtility.GetMaxStatPawn(talksSite.AssociatedFixedCaravan.PawnsListForReading, StatDefOf.NegotiationAbility);

        MultiPartyTalksTopicEventData eventData = new(this.talksSite, this.topicFactions, negotiator);

        MultiPartyTalksTopicEventDef eventDef = DefDatabase<MultiPartyTalksTopicEventDef>.AllDefsListForReading
            .Where(d => MultiPartyTalksUtility.CanTriggerTopicEvent(d, eventType, isSuccessful, eventData))
            .RandomElementWithFallback();

        if (eventDef is null)
            return;

        eventDef.Worker.TryTriggerEvent(eventData);
    }

    private void ApplyCommonReward(TopicRewardData rewardData)
    {
        Faction ofPlayer = Faction.OfPlayer;
        Faction oaFaction = ModUtility.OAFaction;

        if (rewardData.BaselineAdjust != 0f)
            talksSite.AdjustBaselineNegotiationAbility(rewardData.BaselineAdjust);


        if (rewardData.OrganizerGoodwill != 0)
        {
            ofPlayer.TryAffectGoodwillWith(other: talksSite.Faction,
                                           goodwillChange: rewardData.TopicFactionsGoodwill,
                                           reason: OARK_HistoryEventDefOf.OARK_DiplomacyInteraction,
                                           lookTarget: talksSite);
        }

        if (rewardData.TopicFactionsGoodwill != 0)
        {
            foreach (Faction f in topicFactions)
            {
                if (f != oaFaction)
                {
                    ofPlayer.TryAffectGoodwillWith(other: f,
                                                   goodwillChange: rewardData.TopicFactionsGoodwill,
                                                   reason: OARK_HistoryEventDefOf.OARK_DiplomacyInteraction,
                                                   lookTarget: talksSite);
                }
            }
        }

        if (rewardData.OtherFactionsGoodwill != 0)
        {
            foreach (Faction f in talksSite.ParticipantFactions)
            {
                if (f == oaFaction || topicFactions.Contains(f))
                    continue;

                ofPlayer.TryAffectGoodwillWith(other: f,
                                               goodwillChange: rewardData.TopicFactionsGoodwill,
                                               reason: OARK_HistoryEventDefOf.OARK_DiplomacyInteraction,
                                               lookTarget: talksSite);
            }
        }

        FixedCaravan fixedCaravan = talksSite.AssociatedFixedCaravan;
        if (fixedCaravan is null)
            return;

        if (rewardData.IntelligenceDraftCount > 0)
        {
            List<Thing> gifts = OAFrame_ThingUtility.GenerateThingListSplitByStack(OARK_ThingDefOf.OARK_IntelligenceDraft, rewardData.IntelligenceDraftCount);
            OAFrame_FixedCaravanUtility.GiveThings(fixedCaravan, gifts);
        }

        (Pawn negotiator, _) = OAFrame_PawnUtility.GetMaxStatPawn(fixedCaravan.PawnsListForReading, StatDefOf.NegotiationAbility);

        if (negotiator is null)
            return;

        if (rewardData.RoyalFavor != 0)
            negotiator.royalty.GainFavor(talksSite.Faction, rewardData.RoyalFavor);

        if (rewardData.NegotiatorXP != 0)
            negotiator.skills?.Learn(SkillDefOf.Social, rewardData.NegotiatorXP);
    }

    private string RewardDataText(TopicRewardData rewardData)
    {
        GrammarRequest grammarRequest = new();
        grammarRequest.Includes.Add(OARK_RulePackDef.OARK_MultiPartyTalks_TopicOutcome);

        grammarRequest.Rules.AddRange(GrammarUtility.RulesForFaction(KeyLibrary_NamedArgument.FACTION, talksSite.Faction));
        grammarRequest.Rules.Add(new Rule_String(nameof(rewardData.BaselineAdjust), OAFrame_TextUtility.ColoredPercentString(rewardData.BaselineAdjust, reverse: true)));
        grammarRequest.Rules.Add(new Rule_String(nameof(rewardData.AssintPoints), OAFrame_TextUtility.ColoredIntString(rewardData.AssintPoints)));
        grammarRequest.Rules.Add(new Rule_String(nameof(rewardData.RoyalFavor), OAFrame_TextUtility.ColoredIntString(rewardData.RoyalFavor)));
        grammarRequest.Rules.Add(new Rule_String(nameof(rewardData.IntelligenceDraftCount), rewardData.IntelligenceDraftCount.ToString()));
        grammarRequest.Rules.Add(new Rule_String(nameof(rewardData.OrganizerGoodwill), OAFrame_TextUtility.ColoredIntString(rewardData.OrganizerGoodwill)));
        grammarRequest.Rules.Add(new Rule_String(nameof(rewardData.TopicFactionsGoodwill), OAFrame_TextUtility.ColoredIntString(rewardData.TopicFactionsGoodwill)));
        grammarRequest.Rules.Add(new Rule_String(nameof(rewardData.OtherFactionsGoodwill), OAFrame_TextUtility.ColoredIntString(rewardData.OtherFactionsGoodwill)));
        grammarRequest.Rules.Add(new Rule_String(KeyLibrary_NamedArgument.TopicFactionsName, string.Join(", ", topicFactions.Select(f => f.NameColored))));
        grammarRequest.Rules.Add(new Rule_String(nameof(rewardData.NegotiatorXP), rewardData.NegotiatorXP.ToString()));

        grammarRequest.Constants.Add(nameof(rewardData.BaselineAdjust), rewardData.BaselineAdjust.ToString("0.###"));
        grammarRequest.Constants.Add(nameof(rewardData.AssintPoints), rewardData.AssintPoints.ToString());
        grammarRequest.Constants.Add(nameof(rewardData.RoyalFavor), rewardData.RoyalFavor.ToString());
        grammarRequest.Constants.Add(nameof(rewardData.IntelligenceDraftCount), rewardData.IntelligenceDraftCount.ToString());
        grammarRequest.Constants.Add(nameof(rewardData.OrganizerGoodwill), rewardData.OrganizerGoodwill.ToString());
        grammarRequest.Constants.Add(nameof(rewardData.TopicFactionsGoodwill), rewardData.TopicFactionsGoodwill.ToString());
        grammarRequest.Constants.Add(nameof(rewardData.OtherFactionsGoodwill), rewardData.OtherFactionsGoodwill.ToString());

        return GrammarResolver.Resolve("r_text", grammarRequest);
    }

    private string TopicInfoText()
    {
        GrammarRequest grammarRequest = new();
        grammarRequest.Includes.Add(OARK_RulePackDef.OARK_MultiPartyTalks_TopicInfo);

        grammarRequest.Rules.Add(new Rule_String(KeyLibrary_NamedArgument.TopicFactionsName, string.Join(", ", topicFactions.Select(f => f.NameColored))));

        grammarRequest.Constants.Add(nameof(talksSite.TotalTopics), talksSite.TotalTopics.ToString());
        grammarRequest.Constants.Add(nameof(talksSite.TriggeredTopics), talksSite.TriggeredTopics.ToString());
        grammarRequest.Constants.Add(nameof(isSeriousTopic), isSeriousTopic.ToString());

        return GrammarResolver.Resolve("r_text", grammarRequest);
    }

    private struct TopicRewardData
    {
        public float BaselineAdjust { get; set; }
        public int AssintPoints { get; set; }
        public int RoyalFavor { get; set; }
        public int IntelligenceDraftCount { get; set; }
        public int OrganizerGoodwill { get; set; }
        public int TopicFactionsGoodwill { get; set; }
        public int OtherFactionsGoodwill { get; set; }
        public int NegotiatorXP { get; set; }
    }
}