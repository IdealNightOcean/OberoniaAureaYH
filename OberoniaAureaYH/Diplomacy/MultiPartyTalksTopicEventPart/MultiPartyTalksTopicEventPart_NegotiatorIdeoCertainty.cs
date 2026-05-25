using OberoniaAurea_Frame;
using RimWorld;
using System.Text;
using Verse;

namespace OberoniaAurea;

public class MultiPartyTalksTopicEventPart_NegotiatorIdeoCertainty : MultiPartyTalksTopicEventPart
{
    public float certaintyChange;

    public override void ApplyEventOutcome(MultiPartyTalksTopicEventData eventData, StringBuilder resultBuilder)
    {
        if (eventData.Negotiator is null)
            return;

        if (ModsConfig.IdeologyActive && eventData.Negotiator.ideo is not null)
        {
            eventData.Negotiator.ideo.Reassure(certaintyChange);
            resultBuilder.AppendLine("OARK_MultiPartyTalksTopicEventPart_NegotiatorIdeoCertainty".Translate(
                OAFrame_TextUtility.ColoredPercentNamedArgument(certaintyChange, KeyLibrary_NamedArgument.Value),
                eventData.Negotiator.Named(KeyLibrary_NamedArgument.PAWN)));
        }
    }
}