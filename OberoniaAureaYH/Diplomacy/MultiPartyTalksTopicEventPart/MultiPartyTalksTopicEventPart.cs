using System.Text;

namespace OberoniaAurea;

public abstract class MultiPartyTalksTopicEventPart
{
    public abstract void ApplyEventOutcome(MultiPartyTalksTopicEventData eventData, StringBuilder resultBuilder);
}
