using System.Text;

namespace OberoniaAurea;

using static MultiPartyTalksTopicEventWorker;

public abstract class MultiPartyTalksTopicEventPart
{
    public abstract void ApplyEventOutcome(EventData eventData, StringBuilder resultBuilder);
}
