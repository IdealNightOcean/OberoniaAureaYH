using RimWorld;
using System.Text;
using Verse;

namespace OberoniaAurea;

using static MultiPartyTalksTopicEventWorker;

public class MultiPartyTalksTopicEventPart_NegotiatorRandomInspiration : MultiPartyTalksTopicEventPart
{
    public override void ApplyEventOutcome(EventData eventData, StringBuilder resultBuilder)
    {
        if (eventData.Negotiator is null)
            return;

        InspirationDef inspiration = DefDatabase<InspirationDef>.AllDefsListForReading.RandomElement();
        if (inspiration is not null)
        {
            eventData.Negotiator.mindState?.inspirationHandler?.TryStartInspiration(inspiration);
            resultBuilder.AppendLine("OARK_MultiPartyTalksTopicEventPart_NegotiatorRandomInspiration".Translate(
                inspiration.Named(KeyLibrary_NamedArgument.DEF),
                eventData.Negotiator.Named(KeyLibrary_NamedArgument.PAWN)));
        }
    }
}