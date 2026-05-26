using System.Collections.Generic;
using Verse;

namespace OberoniaAurea;

public abstract class MultiPartyTalksTopicEventWorker_MultiOptions : MultiPartyTalksTopicEventWorker
{
    protected abstract IEnumerable<DiaOption> Choices(MultiPartyTalksTopicEventData eventData);

    public override bool TryTriggerEvent(MultiPartyTalksTopicEventData eventData)
    {
        DiaNode rootNode = new(ResolveEventRootNodeText(eventData));

        rootNode.options.AddRange(Choices(eventData));

        Find.WindowStack.Add(new Dialog_NodeTree(rootNode));
        return true;
    }
}