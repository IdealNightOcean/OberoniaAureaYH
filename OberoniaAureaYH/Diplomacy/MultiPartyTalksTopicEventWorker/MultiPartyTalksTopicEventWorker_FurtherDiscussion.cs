using OberoniaAurea_Frame;
using RimWorld;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace OberoniaAurea;

public class MultiPartyTalksTopicEventWorker_FurtherDiscussion : MultiPartyTalksTopicEventWorker_MultiOptions
{
    private const int SucceedGoodwill = 10;
    private const int FailedGoodwill = -10;

    protected override IEnumerable<DiaOption> Choices(MultiPartyTalksTopicEventData eventData)
    {
        yield return new("OARK_TopicEvent_FurtherDiscussion".Translate())
        {
            linkLateBind = () => DiscussionNode(eventData),
        };
        yield return OAFrame_DiaUtility.DefaultIngnoreOption;
    }

    private DiaNode DiscussionNode(MultiPartyTalksTopicEventData eventData)
    {
        StringBuilder nodeTextBuilder = new(64);
        if (Rand.Chance(0.6f))
        {
            nodeTextBuilder.AppendLine("OARK_TopicEvent_FurtherDiscussion_Succeed".Translate(
                eventData.Negotiator.Named(KeyLibrary_NamedArgument.PAWN),
                eventData.PrimaryFaction.Named(KeyLibrary_NamedArgument.FACTION)));
            nodeTextBuilder.AppendLine();

            eventData.TalksSite.AdjustBaselineNegotiationAbility(-0.05f);
            nodeTextBuilder.AppendLine("OARK_MultiPartyTalksTopicEventPart_TalksBaseline".Translate(OAFrame_TextUtility.ColoredPercentNamedArgument(-0.05f, KeyLibrary_NamedArgument.Change, includeSign: true, reverse: true)));

            Faction.OfPlayer.TryAffectGoodwillWith(other: eventData.PrimaryFaction,
                                      goodwillChange: SucceedGoodwill,
                                      reason: OARK_HistoryEventDefOf.OARK_DiplomacyInteraction);
            nodeTextBuilder.AppendLine("OAFrame_FactionGoodwillChange".Translate(
            eventData.PrimaryFaction.Named(KeyLibrary_NamedArgument.FACTION),
            OAFrame_TextUtility.ColoredIntNamedArgument(SucceedGoodwill, KeyLibrary_NamedArgument.Change, includeSign: true)));

        }
        else
        {
            nodeTextBuilder.AppendLine("OARK_TopicEvent_FurtherDiscussion_Failed".Translate(
                eventData.Negotiator.Named(KeyLibrary_NamedArgument.PAWN),
                eventData.PrimaryFaction.Named(KeyLibrary_NamedArgument.FACTION)));
            nodeTextBuilder.AppendLine();

            eventData.TalksSite.AdjustBaselineNegotiationAbility(0.05f);
            nodeTextBuilder.AppendLine("OARK_MultiPartyTalksTopicEventPart_TalksBaseline".Translate(OAFrame_TextUtility.ColoredPercentNamedArgument(0.05f, KeyLibrary_NamedArgument.Change, includeSign: true, reverse: true)));


            Faction.OfPlayer.TryAffectGoodwillWith(other: eventData.PrimaryFaction,
                                                   goodwillChange: FailedGoodwill,
                                                   reason: OARK_HistoryEventDefOf.OARK_DiplomacyInteraction);
            nodeTextBuilder.AppendLine("OAFrame_FactionGoodwillChange".Translate(
                eventData.PrimaryFaction.Named(KeyLibrary_NamedArgument.FACTION),
                OAFrame_TextUtility.ColoredIntNamedArgument(FailedGoodwill, KeyLibrary_NamedArgument.Change, includeSign: true)));

        }

        List<Thing> gifts = OAFrame_ThingUtility.GenerateThingListSplitByStack(OARK_ThingDefOf.OARK_IntelligenceDraft, 50);
        MultiPartyTalksUtility.TryGiveThingsToPlayer(eventData, gifts);
        nodeTextBuilder.AppendLine("OAFrame_GetThings".Translate(GenLabel.ThingsLabel(gifts).Named(KeyLibrary_NamedArgument.ThingsLabel)));

        return OAFrame_DiaUtility.ConfirmDiaNode(nodeTextBuilder.ToTaggedString());
    }
}
