using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
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

    protected DiaOption Option_Confirm(EventData eventData)
    {
        return new DiaOption("Confirm")
        {
            action = () => ApplyEventOutcome(eventData),
            resolveTree = true
        };
    }

    protected virtual IEnumerable<DiaOption> Choices(EventData eventData)
    {

        yield return Option_Confirm(eventData);
    }

    public virtual DiaNode EventRootNode(WorldObject_MultiPartyTalks talksSite,
                                         List<Faction> topicFactions,
                                         Pawn negotiator)
    {
        EventData eventData = new(talksSite, topicFactions, negotiator);
        TaggedString rootText = ResolveEventRootNodeText(eventData);
        DiaNode rootNode = new(rootText);

        rootNode.options.AddRange(Choices(eventData));
        return rootNode;
    }

    protected abstract void ApplyEventOutcome(EventData eventData);

    protected virtual TaggedString ResolveEventRootNodeText(EventData eventData) => eventData.Formatted(Def.description);
    protected virtual void ResolveEventConfirmText(EventData eventData, StringBuilder resultBuilder)
    {
        resultBuilder.AppendLine(eventData.Formatted(Def.confirmText));
    }



    public class EventData
    {
        public WorldObject_MultiPartyTalks TalksSite { get; private set; }
        public List<Faction> TopicFactions { get; private set; }
        public string TopicFactionsName { get; }
        public Pawn Negotiator { get; private set; }

        public EventData(WorldObject_MultiPartyTalks talksSite,
                         List<Faction> topicFactions,
                         Pawn negotiator)
        {
            TalksSite = talksSite;
            TopicFactions = topicFactions;
            Negotiator = negotiator;

            TopicFactionsName = string.Join(", ", topicFactions.Select(f => f.NameColored));
        }

        public string Formatted(string oriString)
        {
            return oriString.Formatted(TalksSite.Named("TalkSite"),
                                        TopicFactionsName.Named("TopicFactionsName"),
                                        Negotiator.Named("NEGOTIATOR"));
        }
    }
}