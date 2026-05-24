using System.Text;
using Verse;

namespace OberoniaAurea;

public class MultiPartyTalksTopicEventWorker_Direct : MultiPartyTalksTopicEventWorker
{
    protected override void ApplyEventOutcome(EventData eventData)
    {
        StringBuilder resultBuilder = new(128);
        ResolveEventConfirmText(eventData, resultBuilder);
        if (!Def.parts.NullOrEmpty())
        {
            resultBuilder.AppendLine();
            foreach (MultiPartyTalksTopicEventPart part in Def.parts)
            {
                part.ApplyEventOutcome(eventData, resultBuilder);
            }
        }

        DiaNode resultNode = new(resultBuilder.ToTaggedString());
        Find.WindowStack.Add(new Dialog_NodeTree(resultNode));
    }
}