using OberoniaAurea_Frame;
using RimWorld;
using System.Text;
using Verse;

namespace OberoniaAurea;

public class MultiPartyTalksTopicEventPart_TalksBaseline : MultiPartyTalksTopicEventPart
{
    public float change;

    public override void ApplyEventOutcome(MultiPartyTalksTopicEventData eventData, StringBuilder resultBuilder)
    {
        if (eventData.TalksSite is null)
            return;

        eventData.TalksSite.AdjustBaselineNegotiationAbility(change);
        resultBuilder.AppendLine("OARK_MultiPartyTalksTopicEventPart_TalksBaseline".Translate(OAFrame_TextUtility.ColoredPercentNamedArgument(change, KeyLibrary_NamedArgument.Value, reverse: true)));
    }
}
