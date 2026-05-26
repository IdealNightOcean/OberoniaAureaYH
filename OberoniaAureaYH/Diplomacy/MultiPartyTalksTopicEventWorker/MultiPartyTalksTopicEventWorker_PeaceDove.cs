using OberoniaAurea_Frame;
using RimWorld;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace OberoniaAurea;

public class MultiPartyTalksTopicEventWorker_PeaceDove : MultiPartyTalksTopicEventWorker_MultiOptions
{
    protected override IEnumerable<DiaOption> Choices(MultiPartyTalksTopicEventData eventData)
    {
        yield return new("OARK_TopicEvent_PeaceDove_Feed".Translate())
        {
            linkLateBind = () => FeedNode(eventData),
        };
        yield return new("OARK_TopicEvent_PeaceDove_Roast".Translate())
        {
            linkLateBind = () => RoastNode(eventData),
        };
        yield return new("OARK_TopicEvent_PeaceDove_Ingnore".Translate())
        {
            resolveTree = true
        };
    }

    private DiaNode FeedNode(MultiPartyTalksTopicEventData eventData)
    {
        StringBuilder nodeTextBuilder = new(64);
        float roll = Rand.Value;
        if (roll < 0.8f)
        {
            nodeTextBuilder.AppendLine("OARK_TopicEvent_PeaceDove_Feed_Succeed".Translate(eventData.Negotiator.Named(KeyLibrary_NamedArgument.PAWN)));
            nodeTextBuilder.AppendLine();

            eventData.Negotiator.skills?.Learn(SkillDefOf.Animals, 1000f);
            nodeTextBuilder.AppendLine("OAFrame_PawnGetSkillXP".Translate(
                SkillDefOf.Animals.Named("SKILL"),
                1000f.Named(KeyLibrary_NamedArgument.Value),
                eventData.Negotiator.Named(KeyLibrary_NamedArgument.PAWN)));

            eventData.Negotiator.needs?.mood?.thoughts.memories.TryGainMemory(ThoughtDefOf.Nuzzled);
        }
        else if (roll < 0.95f)
        {
            nodeTextBuilder.AppendLine("OARK_TopicEvent_PeaceDove_Feed_Neutral".Translate(eventData.Negotiator.Named(KeyLibrary_NamedArgument.PAWN)));
        }
        else
        {
            nodeTextBuilder.AppendLine("OARK_TopicEvent_PeaceDove_Feed_Disaster".Translate(eventData.Negotiator.Named(KeyLibrary_NamedArgument.PAWN)));
            nodeTextBuilder.AppendLine();

            eventData.TalksSite.AdjustBaselineNegotiationAbility(0.05f);
            nodeTextBuilder.AppendLine("OARK_MultiPartyTalksTopicEventPart_TalksBaseline".Translate(
                OAFrame_TextUtility.ColoredPercentNamedArgument(0.05f, KeyLibrary_NamedArgument.Change, reverse: true)));
        }

        return OAFrame_DiaUtility.ConfirmDiaNode(nodeTextBuilder.ToTaggedString());
    }

    private DiaNode RoastNode(MultiPartyTalksTopicEventData eventData)
    {
        StringBuilder nodeTextBuilder = new(64);
        nodeTextBuilder.AppendLine("OARK_TopicEvent_PeaceDove_Roast_Result".Translate(eventData.Negotiator.Named(KeyLibrary_NamedArgument.PAWN)));
        nodeTextBuilder.AppendLine();

        ThingDef mealLavishDef = DefDatabase<ThingDef>.GetNamed("MealLavish");
        List<Thing> gifts = OAFrame_ThingUtility.GenerateThingListSplitByStack(mealLavishDef, 4);
        MultiPartyTalksUtility.TryGiveThingsToPlayer(eventData, gifts);
        nodeTextBuilder.AppendLine("OAFrame_GetThings".Translate(GenLabel.ThingsLabel(gifts).Named(KeyLibrary_NamedArgument.ThingsLabel)));

        return OAFrame_DiaUtility.ConfirmDiaNode(nodeTextBuilder.ToTaggedString());
    }
}