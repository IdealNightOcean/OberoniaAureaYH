using System;
using Verse;

namespace OberoniaAurea;

public abstract class MultiPartyTalksTopicEventWorker
{
    public MultiPartyTalksTopicEventDef Def { get; private set; }

    public static MultiPartyTalksTopicEventWorker GenerateEventWorker(MultiPartyTalksTopicEventDef def)
    {
        MultiPartyTalksTopicEventWorker worker = (MultiPartyTalksTopicEventWorker)Activator.CreateInstance(def.workerClass);
        worker.Def = def;
        return worker;
    }

    public virtual bool CanTriggerEvent(MultiPartyTalksTopicEventData eventData) => true;
    public abstract bool TryTriggerEvent(MultiPartyTalksTopicEventData eventData);

    protected virtual TaggedString ResolveEventRootNodeText(MultiPartyTalksTopicEventData eventData) => eventData.Formatted(Def.description);

}