using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
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

    protected DiaOption Option_Close => new("Close".Translate()) { resolveTree = true };

    protected DiaOption Option_Confirm(MultiPartyTalksTopicEventData eventData)
    {
        return new DiaOption("Confirm")
        {
            action = () => ApplyEventOutcome(eventData),
            resolveTree = true
        };
    }

    protected virtual IEnumerable<DiaOption> Choices(MultiPartyTalksTopicEventData eventData)
    {
        yield return Option_Confirm(eventData);
    }

    public virtual bool CanTriggerEvent(MultiPartyTalksTopicEventData eventData) => true;
    public virtual bool TryTriggerEvent(MultiPartyTalksTopicEventData eventData)
    {
        TaggedString rootText = ResolveEventRootNodeText(eventData);
        DiaNode rootNode = new(rootText);

        rootNode.options.AddRange(Choices(eventData));
        Find.WindowStack.Add(new Dialog_NodeTree(rootNode));
        return true;
    }

    protected abstract void ApplyEventOutcome(MultiPartyTalksTopicEventData eventData);

    protected virtual TaggedString ResolveEventRootNodeText(MultiPartyTalksTopicEventData eventData) => eventData.Formatted(Def.description);

    protected virtual void ResolveEventConfirmText(MultiPartyTalksTopicEventData eventData, StringBuilder resultBuilder) => resultBuilder.AppendLine(eventData.Formatted(Def.confirmText));
}