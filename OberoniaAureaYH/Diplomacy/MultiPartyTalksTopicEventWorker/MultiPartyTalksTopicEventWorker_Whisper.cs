using OberoniaAurea_Frame;
using RimWorld;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace OberoniaAurea;

public class MultiPartyTalksTopicEventWorker_Whisper : MultiPartyTalksTopicEventWorker_MultiOptions
{
    protected override IEnumerable<DiaOption> Choices(MultiPartyTalksTopicEventData eventData)
    {
        yield return new("OARK_TopicEvent_Whisper_Eavesdrop".Translate())
        {
            linkLateBind = () => EavesdropNode(eventData),
        };
        yield return OAFrame_DiaUtility.DefaultIngnoreOption;
    }

    private DiaNode EavesdropNode(MultiPartyTalksTopicEventData eventData)
    {
        StringBuilder nodeTextBuilder = new(64);
        float roll = Rand.Value;
        if (roll < 0.4f)
        {
            nodeTextBuilder.AppendLine("OARK_TopicEvent_Whisper_Succeed".Translate(
                eventData.Negotiator.Named(KeyLibrary_NamedArgument.PAWN),
                eventData.PrimaryFaction.Named(KeyLibrary_NamedArgument.FACTION)));

            List<Thing> gifts = OAFrame_ThingUtility.GenerateThingListSplitByStack(OARK_ThingDefOf.OARK_IntelligenceDraft, 50);
            MultiPartyTalksUtility.TryGiveThingsToPlayer(eventData, gifts);
            nodeTextBuilder.AppendLine("OAFrame_GetThings".Translate(GenLabel.ThingsLabel(gifts).Named(KeyLibrary_NamedArgument.ThingsLabel)));
        }
        else if (roll < 0.7f)
        {
            nodeTextBuilder.AppendLine("OARK_TopicEvent_Whisper_FriendlyChat".Translate(
                eventData.Negotiator.Named(KeyLibrary_NamedArgument.PAWN),
                eventData.PrimaryFaction.Named(KeyLibrary_NamedArgument.FACTION)));
        }
        else
        {
            nodeTextBuilder.AppendLine("OARK_TopicEvent_Whisper_Caught".Translate(
                eventData.Negotiator.Named(KeyLibrary_NamedArgument.PAWN),
                eventData.PrimaryFaction.Named(KeyLibrary_NamedArgument.FACTION)));

            eventData.Negotiator.needs?.mood?.thoughts.memories.TryGainMemory(OARK_ThoughtDefOf.OARK_WhisperCaught);
        }

        return OAFrame_DiaUtility.ConfirmDiaNode(nodeTextBuilder.ToTaggedString());
    }
}