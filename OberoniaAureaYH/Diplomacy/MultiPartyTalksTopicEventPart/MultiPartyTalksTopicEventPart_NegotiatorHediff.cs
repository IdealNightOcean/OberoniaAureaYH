using RimWorld;
using System.Text;
using Verse;

namespace OberoniaAurea;

public class MultiPartyTalksTopicEventPart_NegotiatorHediff : MultiPartyTalksTopicEventPart
{
    public HediffDef hediff;
    public float severity = 1f;

    public override void ApplyEventOutcome(MultiPartyTalksTopicEventData eventData, StringBuilder resultBuilder)
    {
        if (hediff is null || eventData.Negotiator is null)
            return;

        Hediff existingHediff = eventData.Negotiator.health?.hediffSet?.GetFirstHediffOfDef(hediff);
        if (existingHediff is not null)
        {
            existingHediff.Severity += severity;
        }
        else
        {
            eventData.Negotiator.health?.AddHediff(hediff);
            Hediff newHediff = eventData.Negotiator.health?.hediffSet?.GetFirstHediffOfDef(hediff);
            if (newHediff is not null)
            {
                newHediff.Severity = severity;
            }
        }

        resultBuilder.AppendLine("OARK_MultiPartyTalksTopicEventPart_NegotiatorHediff".Translate(
            hediff.Named(KeyLibrary_NamedArgument.DEF),
            severity.Named(KeyLibrary_NamedArgument.Value),
            eventData.Negotiator.Named(KeyLibrary_NamedArgument.PAWN)));
    }
}