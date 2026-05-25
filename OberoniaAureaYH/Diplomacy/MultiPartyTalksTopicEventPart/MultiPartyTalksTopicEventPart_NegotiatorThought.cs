using RimWorld;
using System.Text;
using Verse;

namespace OberoniaAurea;

public class MultiPartyTalksTopicEventPart_NegotiatorThought : MultiPartyTalksTopicEventPart
{
    public ThoughtDef thought;

    public override void ApplyEventOutcome(MultiPartyTalksTopicEventData eventData, StringBuilder resultBuilder)
    {
        if (thought is null || eventData.Negotiator is null)
            return;

        eventData.Negotiator.needs?.mood?.thoughts.memories.TryGainMemory(thought);
        resultBuilder.AppendLine("OARK_MultiPartyTalksTopicEventPart_NegotiatorThought".Translate(
            thought.Named(KeyLibrary_NamedArgument.DEF),
            eventData.Negotiator.Named(KeyLibrary_NamedArgument.PAWN)));
    }
}